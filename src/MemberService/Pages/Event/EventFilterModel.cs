using System;

namespace MemberService.Pages.Event
{
    public class EventFilterModel
    {
        public DateTime? SignedUpBefore { get; set; }

        public int? Priority { get; set; }

        public string Name { get; set; }
        public bool NoOtherSpots { get; set; }
    }
}