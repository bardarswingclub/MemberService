using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Data
{
    public class MemberRole : IdentityRole
    {
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
