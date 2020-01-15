using System;

namespace MemberService.Pages.Event
{
    public class EventFilterModel
    {
        public DateTime? SignedUpBefore { get; set; }

        public int? Priority { get; set; }

        public string Name { get; set; }

        public bool ExcludeAcceptedElsewhere { get; set; }

        public bool ExcludeApprovedElsewhere { get; set; }

        public bool ExcludeRecommendedElsewhere { get; set; }

        public bool OnlyWaitingListElsewhere { get; set; }

        public bool OnlyRejectedElsewhere { get; set; }

        public bool OnlyDeniedElsewhere { get; set; }
    }
}