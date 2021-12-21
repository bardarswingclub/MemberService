namespace MemberService.Pages.Home;




using MemberService.Pages.Signup;

public class IndexModel
{
    public DateTime? SignupOpensAt { get; set; }

    public IReadOnlyCollection<CourseSignupModel> Signups { get; set; } = new List<CourseSignupModel>();

    public string UserId { get; set; }

    public EventsModel PartyModel { get; set; }

    public EventsModel WorkshopModel { get; set; }

    public EventsModel TrainingModel { get; set; }
}
