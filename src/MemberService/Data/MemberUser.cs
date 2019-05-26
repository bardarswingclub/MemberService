using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Data
{
    public class MemberUser : IdentityUser
    {
        [PersonalData]
        public string FullName { get; set; }

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public ICollection<MemberUserRole> UserRoles { get; set; } = new List<MemberUserRole>();

        public ICollection<EventSignup> EventSignups { get; set; } = new List<EventSignup>();

        public bool ExemptFromTrainingFee { get; set; }

        public bool ExemptFromClassesFee { get; set; }
    }
}
