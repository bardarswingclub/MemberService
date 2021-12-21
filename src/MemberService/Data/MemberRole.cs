namespace MemberService.Data;



using Microsoft.AspNetCore.Identity;

public class MemberRole : IdentityRole
{
    public ICollection<UserRole> UserRoles { get; set; }
}
