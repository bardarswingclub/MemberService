using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Data
{
    public class MemberUser : IdentityUser {
        public ICollection<Payment> Payments { get; set; }
    }
}
