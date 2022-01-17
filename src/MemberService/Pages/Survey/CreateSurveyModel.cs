namespace MemberService.Pages.Event.Survey;

using Clave.Expressionify;

public partial class CreateSurveyModel : EventBaseModel
{
    [Expressionify]
    public static CreateSurveyModel Create(Data.Event e) => new()
    {
        EventId = e.Id,
        SemesterId = e.SemesterId,
        EventTitle = e.Title,
        EventDescription = e.Description,
        IsArchived = e.Archived,
        IsCancelled = e.Cancelled,
    };
}
