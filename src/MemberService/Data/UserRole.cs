using Microsoft.AspNetCore.Identity;

namespace MemberService.Data
{
    public class UserRole : IdentityUserRole<string>
    {
        public virtual User User { get; set; }
        public virtual MemberRole Role { get; set; }
    }
}
