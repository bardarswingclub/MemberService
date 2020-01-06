using System;
using System.Collections.Generic;
using System.Linq;
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
    [Route("/Event/{id}/Questions/{action=Index}/{questionId?}")]
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
        public async Task<IActionResult> Index(Guid id, string filter="all")
        {/*
            var e = await _database.Events
                .Include(e => e.Signups)
                .Where(e => e.Signups.)
                .SingleOrDefaultAsync(e => e.Id == id);

            _database.Entry(e)
                .Collection(e => e.Signups)
                .;
                

            var survey = await _database
                .Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .Include(s => s.Responses)
                    .ThenInclude(r => r.Answers)
                .SingleOrDefaultAsync(s => s.Id == id);

            await _database.Entry(survey)
                .Collection(s => s.Responses)
                */
            return View(null);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var model = await _database
                .Events
                .Include(e => e.Survey)
                .ThenInclude(s => s.Questions)
                .ThenInclude(q => q.Options)
                .AsNoTracking()
                .Expressionify()
                .Select(e => SurveyModel.Create(e.Survey))
                .SingleOrDefaultAsync(s => s.EventId == id);

            return View(model);
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
        public async Task<IActionResult> Add(
            Guid id,
            [FromForm] QuestionType type)
        {
            var model = await _database
                .Surveys
                .Include(s => s.Responses)
                .SingleOrDefaultAsync(e => e.Id == id);

            model.Questions.Add(new Question
            {
                Type = type,
                Order = model.Questions.Count
            });

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
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
