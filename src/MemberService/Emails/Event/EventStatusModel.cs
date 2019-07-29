namespace MemberService.Emails.Event
{
    public class EventStatusModel
    {
        public EventStatusModel(string name, string title, string link)
        {
            Name = name;
            Title = title;
            Link = link;
        }

        public string Name { get; }

        public string Title { get; }

        public string Link { get; }
    }
}
