using System.Collections.Generic;

namespace MemberService.Pages.Signup
{
    public class EventsModel
    {
        public IReadOnlyCollection<Data.Event> OpenEvents { get; set; }

        public IReadOnlyCollection<Data.Event> FutureEvents { get; set; }
    }
}
