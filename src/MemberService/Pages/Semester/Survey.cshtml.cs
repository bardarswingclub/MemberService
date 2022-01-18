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

namespace MemberService.Pages.Semester;

[Authorize(nameof(Policy.CanViewSurvey))]
public partial class SurveyModel : PageModel
{
    private readonly MemberContext _database;

    public Guid Id { get; set; }

    public Guid SemesterId { get; set; }

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
                SemesterId = s.Semester.Id,
                Title = s.Title,
                Description = s.Description,
                Questions = s.Questions
                    .Select(q => QuestionModel.Create(q))
                    .ToList(),
                Responses = s.Responses.AsQueryable()
                    .SelectMany(r => r.Answers.Select(a => ResponseModel.Create(r, a)))
                    .Where(GetFilter(filter))
                    .ToList()
            })
            .FirstOrDefaultAsync(s => s.SemesterId == id);

        if (model == null)
        {
            return NotFound();
        }

        Id = model.Id;
        SemesterId = model.SemesterId;
        Title = model.Title;
        Questions = model.Questions;
        Responses = model.Responses;
        Filter = filter;

        return Page();
    }


    private static Expression<Func<ResponseModel, bool>> GetFilter(string filter) =>
        filter switch
        {
            "member" => r => r.HasPayedMembershipThisYear,
            "training" => r => r.HasPayedTrainingFeeThisSemester,
            "classes" => r => r.HasPayedClassesFeeThisSemester,
            _ => r => true
        };

    public partial class ResponseModel
    {
        public string Name { get; set; }

        public string UserId { get; set; }

        public bool HasPayedMembershipThisYear { get; set; }

        public bool HasPayedTrainingFeeThisSemester { get; set; }

        public bool HasPayedClassesFeeThisSemester { get; set; }

        public Guid OptionId { get; set; }

        [Expressionify]
        public static ResponseModel Create(Response r, QuestionAnswer a) =>
            new()
            {
                UserId = r.UserId,
                Name = r.User.FullName,
                HasPayedMembershipThisYear = r.User.HasPayedMembershipThisYear(),
                HasPayedTrainingFeeThisSemester = r.User.HasPayedTrainingFeeThisSemester(),
                HasPayedClassesFeeThisSemester = r.User.HasPayedClassesFeeThisSemester(),
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
