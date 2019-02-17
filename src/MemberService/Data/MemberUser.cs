using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Data
{
    public class MemberUser : IdentityUser
    {
        [PersonalData]
        public string FullName { get; set; }

        public ICollection<Payment> Payments { get; set; }

        public ICollection<MemberUserRole> UserRoles { get; set; }
    }
}
