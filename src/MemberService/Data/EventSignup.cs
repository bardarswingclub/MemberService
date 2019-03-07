using System;
using System.ComponentModel.DataAnnotations;

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
    }
}
