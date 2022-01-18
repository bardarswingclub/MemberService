using System.ComponentModel;
using System.Linq.Expressions;

using Clave.Expressionify;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Event;

[Authorize(nameof(Policy.CanViewSurvey))]
public partial class SurveyModel : PageModel
{
    private readonly MemberContext _database;

    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public string Title { get; set; }

    public string Filter { get; set; }

    public IReadOnlyList<QuestionModel> Questions { get; set; }

    public IReadOnlyCollection<ResponseModel> Responses { get; set; }

    public SurveyModel(MemberContext database)
    {
        _database = database;
    }

    public async Task<IActionResult> OnGet(Guid id, string filter = "all")
    {
        var model = await _database.Surveys
            .Expressionify()
            .Select(s => new
            {
                Id = s.Id,
                EventId = s.Event.Id,
                Title = s.Title,
                Description = s.Description,
                Questions = s.Questions
                    .Select(q => QuestionModel.Create(q))
                    .ToList(),
                Responses = s.Event.Signups.AsQueryable()
                    .Where(GetFilter(filter))
                    .SelectMany(es => es.Response.Answers.Select(a => ResponseModel.Create(es, a)))
                    .ToList()
            })
            .FirstOrDefaultAsync(s => s.EventId == id);

        if (model == null)
        {
            return NotFound();
        }

        Id = model.Id;
        EventId = model.EventId;
        Title = model.Title;
        Questions = model.Questions;
        Responses = model.Responses;
        Filter = filter;

        return Page();
    }

    private static Expression<Func<EventSignup, bool>> GetFilter(string filter) =>
        filter switch
        {
            "paid" => es => es.Status == Status.AcceptedAndPayed,
            "approved" => es => es.Status == Status.AcceptedAndPayed || es.Status == Status.Approved,
            _ => es => true
        };

    public partial class ResponseModel
    {
        public string Name { get; set; }

        public string UserId { get; set; }

        public Status Status { get; set; }

        public Guid OptionId { get; set; }

        [Expressionify]
        public static ResponseModel Create(EventSignup es, QuestionAnswer a) =>
            new()
            {
                UserId = es.Response.UserId,
                Name = es.Response.User.FullName,
                Status = es.Status,
                OptionId = a.OptionId
            };
    }

    public partial class QuestionModel
    {
        public Guid Id { get; set; }

        public QuestionType Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyList<OptionModel> Options { get; set; }

        [Expressionify]
        public static QuestionModel Create(Question q)
            => new()
            {
                Id = q.Id,
                Type = q.Type,
                Title = q.Title,
                Description = q.Description,
                Options = q.Options
                    .Select(o => OptionModel.Create(o))
                    .ToList()
            };

        public partial class OptionModel
        {
            public Guid Id { get; set; }

            [DisplayName("Svaralternativ")]
            public string Title { get; set; }

            [DisplayName("Beskrivelse")]
            public string Description { get; set; }

            [Expressionify]
            public static OptionModel Create(QuestionOption o)
                => new()
                {
                    Id = o.Id,
                    Title = o.Title,
                    Description = o.Description
                };
        }
    }
}
