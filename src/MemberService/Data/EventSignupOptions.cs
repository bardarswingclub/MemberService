using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberService.Data
{
    public class EventSignupOptions
    {
        public Guid Id { get; set; }

        [ForeignKey(nameof(Id))]
        public Event Event { get; set; }

        public DateTime? SignupOpensAt { get; set; }

        public DateTime? SignupClosesAt { get; set; }

        public bool RequiresMembershipFee { get; set; }

        public bool RequiresTrainingFee { get; set; }

        public bool RequiresClassesFee { get; set; }

        public decimal PriceForMembers { get; set; }

        public decimal PriceForNonMembers { get; set; }

        public bool AllowPartnerSignup { get; set; }

        public bool RoleSignup { get; set; }
    }
}