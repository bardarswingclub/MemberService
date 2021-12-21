namespace MemberService.Pages.Event.Survey;





using Clave.Expressionify;

public partial class SurveyModel : EventBaseModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public IReadOnlyList<QuestionModel> Questions { get; set; }

    [Expressionify]
    public static SurveyModel Create(Data.Event e) =>
    new()
    {
        Id = e.Survey.Id,
        EventId = e.Id,
        SemesterId = e.SemesterId,
        EventTitle = e.Title,
        EventDescription = e.Description,
        Title = e.Survey.Title,
        Description = e.Survey.Description,
        Questions = e.Survey.Questions
            .Select(q => QuestionModel.Create(q))
            .ToList()
    };
}
