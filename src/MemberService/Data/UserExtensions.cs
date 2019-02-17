using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Data
{
    public static class UserExtensions
    {
        public static async Task SeedUserRoles(this UserManager<MemberUser> userManager, string email)
        {
            await userManager.EnsureUserHasRole(email, Roles.ADMIN);
        }

        public static async Task SeedRoles(this RoleManager<MemberRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Roles.ADMIN))
            {
                var role = new MemberRole();
                role.Name = Roles.ADMIN;
                await roleManager.CreateAsync(role);
            }


            if (!await roleManager.RoleExistsAsync(Roles.COORDINATOR))
            {
                var role = new MemberRole();
                role.Name = Roles.COORDINATOR;
                await roleManager.CreateAsync(role);
            }
        }
        public static async Task<bool> EnsureUserHasRole(this UserManager<MemberUser> userManager, string email, string role)
        {
            if (await userManager.FindByEmailAsync(email) is MemberUser user)
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                    return true;
                }
            }

            return false;
        }

        public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
            => roles.Any(user.IsInRole);
    }
}