namespace MemberService.Pages.Event.Survey;





using Clave.Expressionify;

using MemberService.Data;
using MemberService.Data.ValueTypes;

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
}
