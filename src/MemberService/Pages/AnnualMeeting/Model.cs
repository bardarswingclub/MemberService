using System;

namespace MemberService.Pages.AnnualMeeting
{
    public class Model
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string MeetingInvitation { get; set; }

        public string MeetingInfo { get; set; }

        public string MeetingSummary { get; set; }

        public bool IsMember { get; set; }

        public DateTime MeetingStartsAt { get; set; }

        public bool HasStarted { get; set; }

        public bool HasEnded { get; set; }
    }
}