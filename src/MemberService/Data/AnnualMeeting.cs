using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberService.Data
{
    public class AnnualMeeting
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string MeetingInvitation { get; set; }

        public string MeetingInfo { get; set; }

        public string MeetingSummary { get; set; }

        public DateTime MeetingStartsAt { get; set; }

        public DateTime MeetingEndsAt { get; set; }

        public ICollection<AnnualMeetingAttendee> Attendees { get; set; } = new List<AnnualMeetingAttendee>();

        public Guid? SurveyId { get; set; }

        public Survey Survey { get; set; } = new();
    }
}