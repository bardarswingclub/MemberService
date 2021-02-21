using System;

namespace MemberService.Pages.AnnualMeeting
{
    public class Model
    {
        public string Title { get; set; }

        public string MeetingInvitation { get; set; }

        public string MeetingInfo { get; set; }

        public string MeetingSummary { get; set; }

        public bool IsMember { get; set; }

        public DateTime MeetingStartsAt { get; set; }
    }
}