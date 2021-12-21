namespace MemberService.Pages.Signup;




public class EventsModel
{
    public String Title { get; set; }

    public IReadOnlyCollection<EventModel> OpenEvents { get; set; }

    public IReadOnlyCollection<EventModel> FutureEvents { get; set; }
}
