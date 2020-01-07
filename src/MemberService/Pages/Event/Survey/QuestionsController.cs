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

namespace MemberService.Pages.Event.Survey
{
    [Authorize(nameof(Policy.IsInstructor))]
    [Route("/Event/{eventId}/Questions/{action=Index}/{questionId?}")]
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
        public async Task<IActionResult> Index(Guid eventId, string filter="all")
        {
            var model = await _database
                .Events
                .Include(e => e.Survey)
                .ThenInclude(s => s.Questions)
                .ThenInclude(q => q.Options)
                .Include(e => e.Signups)
                .ThenInclude(s => s.Response)
                .ThenInclude(r => r.Answers)
                .Include(e => e.Signups)
                .ThenInclude(s => s.Response)
                .ThenInclude(r => r.User)
                .Expressionify()
                .Where(e => e.SurveyId != null)
                .Select(e => SurveyResultModel.Create(e, filter, GetFilter(filter)))
                .FirstOrDefaultAsync(s => s.EventId == eventId);

            if (model == null)
            {
                var createModel = await _database.Events
                    .Select(e => new CreateSurveyModel
                    {
                        EventId = e.Id,
                        EventTitle = e.Title,
                        EventDescription = e.Description
                    })
                    .FirstOrDefaultAsync(e => e.EventId == eventId);

                return View("Create", createModel);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid eventId)
        {
            var model = await _database
                .Events
                .Include(e => e.Survey)
                .ThenInclude(s => s.Questions)
                .ThenInclude(q => q.Options)
                .AsNoTracking()
                .Expressionify()
                .Where(e => e.SurveyId != null)
                .Select(e => SurveyModel.Create(e))
                .FirstOrDefaultAsync(s => s.EventId == eventId);

            if (model == null)
            {
                var createModel = await _database.Events
                    .Select(e => new CreateSurveyModel
                    {
                        EventId = e.Id,
                        EventTitle = e.Title,
                        EventDescription = e.Description
                    })
                    .FirstOrDefaultAsync(e => e.EventId == eventId);

                return View("Create", createModel);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
        public async Task<IActionResult> Create(Guid eventId)
        {
            var ev = await _database.Events.FirstOrDefaultAsync(e => e.Id == eventId);

            ev.Survey = new Data.Survey
            {
                Title = ev.Title,
                Description = ev.Description
            };

            await _database.SaveChangesAsync();
            
            return RedirectToAction(nameof(Edit), new { eventId });
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
        public async Task<IActionResult> Add(
            Guid eventId,
            [FromForm] QuestionType type)
        {
            var ev = await _database.Events.FirstOrDefaultAsync(s => s.Id == eventId);

            var model = await _database
                .Surveys
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(e => e.Id == ev.SurveyId);

            model.Questions.Add(new Question
            {
                Type = type,
                Order = model.Questions.Count
            });

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { eventId });
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
        public async Task<IActionResult> Save(
            Guid eventId,
            Guid questionId,
            QuestionInput input,
            [FromForm] string action)
        {
            var ev = await _database.Events.FirstOrDefaultAsync(s => s.Id == eventId);

            var question = await _database.Questions
                .Include(q => q.Options)
                .Where(q => q.SurveyId == ev.SurveyId)
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

            return RedirectToAction(nameof(Edit), new { eventId });
        }

        private static Expression<Func<EventSignup, bool>> GetFilter(string filter) =>
            filter switch
            {
                "paid" => es => es.Status == Status.AcceptedAndPayed,
                "approved" => es => es.Status == Status.AcceptedAndPayed || es.Status == Status.Approved,
                _ => es => true
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
