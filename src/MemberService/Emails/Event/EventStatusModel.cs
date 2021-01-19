namespace MemberService.Emails.Event
{
    public class EventStatusModel
    {
        public EventStatusModel(string title, string link)
        {
            Title = title;
            Link = link;
        }

        public string Title { get; }

        public string Link { get; }
    }
}
