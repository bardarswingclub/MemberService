using Clave.Expressionify;

namespace MemberService.Pages.Event.Survey
{
    public partial class CreateSurveyModel : EventBaseModel
    {
        [Expressionify]
        public static CreateSurveyModel Create(Data.Event e) => new CreateSurveyModel
        {
            EventId = e.Id,
            SemesterId = e.SemesterId,
            EventTitle = e.Title,
            EventDescription = e.Description,
            IsArchived = e.Archived
        };
    }
}