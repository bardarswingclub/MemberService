using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberService.Data
{
    public class Payment
    {
        public string Id { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public DateTime PayedAtUtc { get; set; }

        public string StripeChargeId { get; set; }

        public string ManualPayment { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Description { get; set; }

        public bool IncludesMembership { get; set; }

        public bool IncludesTraining { get; set; }

        public bool IncludesClasses { get; set; }

        public bool Refunded { get; set; }

        [InverseProperty(nameof(Data.EventSignup.Payment))]
        public EventSignup EventSignup { get; set; }
    }
}
