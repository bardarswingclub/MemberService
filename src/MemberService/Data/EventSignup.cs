using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MemberService.Data.ValueTypes;

namespace MemberService.Data
{
    public class EventSignup
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public Event Event { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }

        [Required]
        public DateTime SignedUpAt { get; set; }

        public DanceRole Role { get; set; }

        public string PartnerEmail { get; set; }

        public User Partner { get; set; }

        public int Priority { get; set; }

        public Status Status { get; set; }

        public string PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }

        public ICollection<EventSignupAuditEntry> AuditLog { get; set; } = new List<EventSignupAuditEntry>();

        public ICollection<Presence> Presence { get; set; } = new List<Presence>();

        public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
    }
}
