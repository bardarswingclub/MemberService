namespace MemberService.Data;

using Microsoft.AspNetCore.Identity;

public class UserRole : IdentityUserRole<string>
{
    public virtual User User { get; set; }
    public virtual MemberRole Role { get; set; }
}
