namespace MemberService.Pages.Survey;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
[Route("/Survey/{id}/{action=Index}/{questionId?}")]
public class SurveyController : Controller
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;

    public SurveyController(
        MemberContext database,
        UserManager<User> userManager)
    {
        _database = database;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanEditSurvey))]
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await _database
            .Surveys
            .Where(s => s.Id == id)
            .Select(s => SurveyModel.Create(s))
            .FirstOrDefaultAsync();

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanCreateSurvey))]
    public async Task<IActionResult> CreateEventSurvey(Guid id)
    {
        var model = await _database.Events.FindAsync(id);

        return View("Create", new CreateSurveyModel
        {
            EventId = model.Id,
            Title = model.Title,
            IsArchived = model.Archived
        });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanCreateSurvey))]
    public async Task<IActionResult> CreateEventSurvey(Guid id, [FromForm] string description)
    {
        var @event = await _database.Events.FindAsync(id);

        if (@event.SurveyId is null)
        {
            @event.Survey = new Survey
            {
                Title = @event.Title,
                Description = description
            };

            await _database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Edit), new { id = @event.SurveyId });
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanCreateSurvey))]
    public async Task<IActionResult> CreateSemesterSurvey(Guid id)
    {
        var model = await _database.Semesters.FindAsync(id);

        return View("Create", new CreateSurveyModel
        {
            SemesterId = model.Id,
            Title = model.Title,
            IsArchived = !model.IsActive()
        });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanCreateSurvey))]
    public async Task<IActionResult> CreateSemesterSurvey(Guid id, [FromForm] string description)
    {
        var semester = await _database.Semesters.FindAsync(id);

        if (semester.SurveyId is null)
        {
            semester.Survey = new Survey
            {
                Title = semester.Title,
                Description = description
            };

            await _database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Edit), new { id = semester.SurveyId });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanEditSurvey))]
    public async Task<IActionResult> Add(
        Guid id,
        [FromForm] QuestionType type)
    {
        var model = await _database.Surveys.FindAsync(id);

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
        var question = await _database.Questions
            .Include(q => q.Options)
            .Where(q => q.SurveyId == id)
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
                var answers = await _database.QuestionAnswers.Where(a => a.OptionId == option.Id).ToListAsync();
                foreach (var answer in answers)
                {
                    _database.QuestionAnswers.Remove(answer);
                }
                question.Options.Remove(option);
            }
        }

        if (action == "delete")
        {
            var answers = await _database.QuestionAnswers.Where(a => a.Option.QuestionId == questionId).ToListAsync();
            foreach(var answer in answers)
            {
                _database.QuestionAnswers.Remove(answer);
            }
            _database.Questions.Remove(question);
        }
        else if (action == "add-option")
        {
            question.Options.Add(new QuestionOption());
        }

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id });
    }
}
