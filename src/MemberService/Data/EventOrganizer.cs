namespace MemberService.Data
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    using MemberService.Data.ValueTypes;

    public class EventOrganizer
    {
        public Guid EventId { get; set; }

        public Event Event { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        public OrganizerType Type { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string UpdatedBy { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public User UpdatedByUser { get; set; }
    }
}