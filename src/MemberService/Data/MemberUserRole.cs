using Microsoft.AspNetCore.Identity;

namespace MemberService.Data
{
    public class MemberUserRole : IdentityUserRole<string>
    {
        public virtual MemberUser User { get; set; }
        public virtual MemberRole Role { get; set; }
    }
}
