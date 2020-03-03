using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using MemberService.Data.ValueTypes;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Data
{
    public static class UserExtensions
    {
        public static async Task SeedUserRoles(this UserManager<User> userManager, params string[] emails)
        {
            foreach(var email in emails)
            {
                await userManager.EnsureUserHasRole(email.Trim(), Roles.Admin);
            }
        }

        public static async Task SeedRoles(this RoleManager<MemberRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                await roleManager.CreateAsync(new MemberRole
                {
                    Name = Roles.Admin
                });
            }

            if (!await roleManager.RoleExistsAsync(Roles.Coordinator))
            {
                await roleManager.CreateAsync(new MemberRole
                {
                    Name = Roles.Coordinator
                });
            }

            if (!await roleManager.RoleExistsAsync(Roles.Instructor))
            {
                await roleManager.CreateAsync(new MemberRole
                {
                    Name = Roles.Instructor
                });
            }
        }

        public static async Task<bool> EnsureUserHasRole(this UserManager<User> userManager, string email, string role)
        {
            if (await userManager.FindByEmailAsync(email) is User user)
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                    return true;
                }
            }

            return false;
        }

        public static bool IsInAnyRole(this IPrincipal user, params string[] roles)
            => roles.Any(user.IsInRole);

        public static string GetFullName(this ClaimsPrincipal user) => user.FindFirstValue("FullName") ?? user.Identity.Name;
    }
}