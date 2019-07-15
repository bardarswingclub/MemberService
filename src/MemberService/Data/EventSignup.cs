using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberService.Data
{
    public class EventSignup
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public Event Event { get; set; }

        public string UserId { get; set; }

        public MemberUser User { get; set; }

        [Required]
        public DateTime SignedUpAt { get; set; }

        public DanceRole Role { get; set; }

        public string PartnerEmail { get; set; }

        public int Priority { get; set; }

        public Status Status { get; set; }

        public string PaymentId { get; set; }

        [ForeignKey(nameof(PaymentId))]
        public Payment Payment { get; set; }

        public ICollection<EventSignupAuditEntry> AuditLog { get; set; } = new List<EventSignupAuditEntry>();
    }
}
