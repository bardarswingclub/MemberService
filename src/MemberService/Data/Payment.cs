using System;
using System.ComponentModel.DataAnnotations;

namespace MemberService.Data
{
    public class Payment
    {
        public string Id { get; set; }

        [Required]
        public MemberUser User { get; set; }

        [Required]
        public DateTime PayedAt { get; set; }

        public string StripeChargeId { get; set; }

        public string ManualPayment { get; set; }

        [Required]
        public long Amount { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IncludesMembership { get; set; }

        public bool IncludesTraining { get; set; }

        public bool IncludesClasses { get; set; }

        public bool Refunded { get; set; }
    }
}