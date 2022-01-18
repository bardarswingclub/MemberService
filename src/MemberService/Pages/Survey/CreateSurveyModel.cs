namespace MemberService.Pages.Survey;

public class CreateSurveyModel
{
    public Guid? SemesterId { get; set; }

    public Guid? EventId { get; set; }

    public string Title { get; set; }

    public bool IsArchived { get; set; }
}
