using System.Collections.Generic;

namespace MemberService.Pages.Signup
{
    public class EventsModel
    {
        public IReadOnlyCollection<EventModel> OpenEvents { get; set; }

        public IReadOnlyCollection<EventModel> FutureEvents { get; set; }
    }
}
