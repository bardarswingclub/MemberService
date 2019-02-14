using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Data
{
    public static class UserRoleSeeder
    {
        public static async Task SeedUserRoles(this UserManager<MemberUser> userManager)
        {
            if (await userManager.FindByEmailAsync("gundersen@gmail.com") is MemberUser user)
            {
                if (!await userManager.IsInRoleAsync(user, Roles.ADMIN))
                {
                    await userManager.AddToRoleAsync(user, Roles.ADMIN);
                }
            }
        }

        public static async Task SeedRoles(this RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Roles.ADMIN))
            {
                IdentityRole role = new IdentityRole();
                role.Name = Roles.ADMIN;
                IdentityResult roleResult = await roleManager.CreateAsync(role);
            }


            if (!await roleManager.RoleExistsAsync(Roles.COORDINATOR))
            {
                IdentityRole role = new IdentityRole();
                role.Name = Roles.COORDINATOR;
                IdentityResult roleResult = await roleManager.CreateAsync(role);
            }
        }
    }
}