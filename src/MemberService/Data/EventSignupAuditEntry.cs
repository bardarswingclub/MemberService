using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberService.Data
{
    public class EventSignupAuditEntry
    {
        public Guid Id { get; set; }

        public Guid EventSignupId { get; set; }

        [ForeignKey(nameof(EventSignupId))]
        public EventSignup EventSignup { get; set; }

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public string Message { get; set; }

        public DateTime OccuredAtUtc { get; set; }
    }
}