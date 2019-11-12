using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clave.Expressionify;
using Clave.ExtensionMethods;
using MemberService.Auth;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Event.Questions
{
    [Authorize(nameof(Policy.IsInstructor))]
    [Route("/Event/{id}/Questions/{action=Index}/{questionId?}")]
    public class QuestionsController : Controller
    {
        private readonly MemberContext _database;
        private readonly UserManager<MemberUser> _userManager;

        public QuestionsController(
            MemberContext database,
            UserManager<MemberUser> userManager)
        {
            _database = database;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid id, string filter="all")
        {
            var model = await _database
                .Events
                .Include(e => e.Questions)
                .ThenInclude(q => q.Options)
                .ThenInclude(s => s.Answers)
                .ThenInclude(a => a.Signup)
                .ThenInclude(s => s.User)
                .AsNoTracking()
                .Expressionify()
                .Select(e => QuestionsModel.Create(e, filter))
                .SingleOrDefaultAsync(s => s.EventId == id);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var model = await _database
                .Events
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Options)
                .AsNoTracking()
                .Expressionify()
                .Select(e => QuestionsModel.Create(e, "all"))
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
                .Events
                .Include(e => e.Questions)
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
                .Where(q => q.EventId == id)
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
