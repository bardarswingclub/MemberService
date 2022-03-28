namespace MemberService.Components.SurveyResponse;

using System.Security.Claims;
using System.Threading.Tasks;

using Clave.Expressionify;

using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;

public partial class SurveyResponseViewComponent : ViewComponent
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;

    public SurveyResponseViewComponent(
        MemberContext database,
        UserManager<User> userManager)
    {
        _database = database;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid? id, bool disabled = false)
    {
        if (id is null)
        {
            return new ContentViewComponentResult(string.Empty);
        }

        var userId = _userManager.GetUserId(User as ClaimsPrincipal);

        var questions = await _database.Surveys
            .Expressionify()
            .Where(s => s.Id == id)
            .SelectMany(s => s.Questions.Select(q => Model.SignupQuestion.Create(q, userId)))
            .ToListAsync();

        return View(new Model(questions, disabled));
    }

    public partial record Model(IReadOnlyList<Model.SignupQuestion> Questions, bool Disabled)
    {
        public partial record SignupQuestion
        {
            public Guid Id { get; set; }

            public QuestionType Type { get; set; }

            public string Title { get; set; }

            public string Description { get; set; }

            public IReadOnlyList<Option> Options { get; set; }

            [Expressionify]
            public static SignupQuestion Create(Question q, string userId)
                => new()
                {
                    Id = q.Id,
                    Type = q.Type,
                    Title = q.Title,
                    Description = q.Description,
                    Options = q.Options
                        .Select(o => Option.Create(o, userId))
                        .ToList()
                };

            public partial record Option
            {
                public Guid Id { get; set; }

                public string Title { get; set; }

                public string Description { get; set; }

                public bool Checked { get; set; }

                [Expressionify]
                public static Option Create(QuestionOption o, string userId)
                    => new()
                    {
                        Id = o.Id,
                        Title = o.Title,
                        Description = o.Description,
                        Checked = o.Answers.Any(a => a.Response.UserId == userId),
                    };
            }
        }

    }
}
