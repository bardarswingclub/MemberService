using System;

namespace MemberService.Data
{
    public class AnnualMeetingAttendee
    {
        public Guid Id { get; set; }

        public Guid AnnualMeetingId { get; set; }

        public AnnualMeeting AnnualMeeting { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastVisited { get; set; }

        public int Visits { get; set; }
    }
}