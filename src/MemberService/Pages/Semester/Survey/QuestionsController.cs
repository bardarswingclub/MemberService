namespace MemberService.Pages.Semester.Survey;




using System.Linq.Expressions;


using Clave.Expressionify;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
[Route("/Semester/{id}/Questions/{action=Index}/{questionId?}")]
public class QuestionsController : Controller
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;

    public QuestionsController(
        MemberContext database,
        UserManager<User> userManager)
    {
        _database = database;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanViewSurvey))]
    public async Task<IActionResult> Index(Guid id, string filter = "all")
    {
        var model = await _database
            .Semesters
            .Expressionify()
            .Where(s => s.SurveyId != null)
            .Select(s => SurveyResultModel.Create(s, filter, GetFilter(filter)))
            .FirstOrDefaultAsync(s => s.SemesterId == id);

        if (model == null)
        {
            var createModel = await _database.Semesters
                .Select(s => new CreateSurveyModel
                {
                    SemesterId = s.Id,
                    SemesterTitle = s.Title
                })
                .FirstOrDefaultAsync(e => e.SemesterId == id);

            return View("Create", createModel);
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanEditSurvey))]
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await _database
            .Semesters
            .AsNoTracking()
            .Expressionify()
            .Where(s => s.SurveyId != null)
            .Select(s => SurveyModel.Create(s))
            .FirstOrDefaultAsync(s => s.SemesterId == id);

        if (model == null)
        {
            var createModel = await _database.Semesters
                .Select(s => new CreateSurveyModel
                {
                    SemesterId = s.Id,
                    SemesterTitle = s.Title
                })
                .FirstOrDefaultAsync(e => e.SemesterId == id);

            return View("Create", createModel);
        }

        return View(model);
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanCreateSurvey))]
    public async Task<IActionResult> Create(Guid id, [FromForm] string description)
    {
        var semester = await _database.Semesters.FirstOrDefaultAsync(e => e.Id == id);

        semester.Survey = new Data.Survey
        {
            Title = semester.Title,
            Description = description
        };

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanEditSurvey))]
    public async Task<IActionResult> Add(
        Guid id,
        [FromForm] QuestionType type)
    {
        var semester = await _database.Semesters.FirstOrDefaultAsync(s => s.Id == id);

        var model = await _database
            .Surveys
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(e => e.Id == semester.SurveyId);

        model.Questions.Add(new Question
        {
            Type = type,
            Order = model.Questions.Count
        });

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanEditSurvey))]
    public async Task<IActionResult> Save(
        Guid id,
        Guid questionId,
        QuestionInput input,
        [FromForm] string action)
    {
        var semester = await _database.Semesters.FirstOrDefaultAsync(s => s.Id == id);

        var question = await _database.Questions
            .Include(q => q.Options)
            .Where(q => q.SurveyId == semester.SurveyId)
            .SingleAsync(q => q.Id == questionId);

        if (question == null)
        {
            return NotFound();
        }

        question.Title = input.Title;
        question.Description = input.Description;

        foreach (var (o, option) in input.Options.Join(question.Options, o => o.Id, o => o.Id))
        {
            option.Title = o.Title;
            option.Description = o.Description;
            option.Order = input.Options.IndexOf(o);

            if (o.Action == "delete")
            {
                question.Options.Remove(option);
            }
        }

        if (action == "delete")
        {
            _database.Questions.Remove(question);
        }
        else if (action == "add-option")
        {
            question.Options.Add(new QuestionOption());
        }

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id });
    }

    private static Expression<Func<ResponseModel, bool>> GetFilter(string filter) =>
        filter switch
        {
            "member" => r => r.HasPayedMembershipThisYear,
            "training" => r => r.HasPayedTrainingFeeThisSemester,
            "classes" => r => r.HasPayedClassesFeeThisSemester,
            _ => r => true
        };
}

public class QuestionInput
{
    public string Title { get; set; }

    public string Description { get; set; }

    public IList<OptionInput> Options { get; set; } = new List<OptionInput>();

    public class OptionInput
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Action { get; set; }
    }
}
