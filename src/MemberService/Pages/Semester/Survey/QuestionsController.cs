using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clave.Expressionify;
using Clave.ExtensionMethods;
using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Semester.Survey
{
    [Authorize(nameof(Policy.IsInstructor))]
    [Route("/Semester/{semesterId}/Questions/{action=Index}/{questionId?}")]
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
        public async Task<IActionResult> Index(Guid semesterId, string filter="all")
        {
            var model = await _database
                .Semesters
                .Expressionify()
                .Where(s => s.SurveyId != null)
                .Select(s => SurveyResultModel.Create(s, filter, GetFilter(filter)))
                .FirstOrDefaultAsync(s => s.SemesterId == semesterId);

            if (model == null)
            {
                var createModel = await _database.Semesters
                    .Select(s => new CreateSurveyModel
                    {
                        SemesterId = s.Id,
                        SemesterTitle = s.Title
                    })
                    .FirstOrDefaultAsync(e => e.SemesterId == semesterId);

                return View("Create", createModel);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid semesterId)
        {
            var model = await _database
                .Semesters
                .AsNoTracking()
                .Expressionify()
                .Where(s => s.SurveyId != null)
                .Select(s => SurveyModel.Create(s))
                .FirstOrDefaultAsync(s => s.SemesterId == semesterId);

            if (model == null)
            {
                var createModel = await _database.Semesters
                    .Select(s => new CreateSurveyModel
                    {
                        SemesterId = s.Id,
                        SemesterTitle = s.Title
                    })
                    .FirstOrDefaultAsync(e => e.SemesterId == semesterId);

                return View("Create", createModel);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
        public async Task<IActionResult> Create(Guid semesterId, [FromForm] string description)
        {
            var semester = await _database.Semesters.FirstOrDefaultAsync(e => e.Id == semesterId);

            semester.Survey = new Data.Survey
            {
                Title = semester.Title,
                Description = description
            };

            await _database.SaveChangesAsync();
            
            return RedirectToAction(nameof(Edit), new { semesterId });
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
        public async Task<IActionResult> Add(
            Guid semesterId,
            [FromForm] QuestionType type)
        {
            var semester = await _database.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);

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

            return RedirectToAction(nameof(Edit), new { semesterId });
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
        public async Task<IActionResult> Save(
            Guid semesterId,
            Guid questionId,
            QuestionInput input,
            [FromForm] string action)
        {
            var semester = await _database.Semesters.FirstOrDefaultAsync(s => s.Id == semesterId);

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

            return RedirectToAction(nameof(Edit), new { semesterId });
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
}
