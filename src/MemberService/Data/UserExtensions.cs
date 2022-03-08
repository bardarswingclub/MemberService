namespace MemberService.Data;


using System.Security.Claims;

using Clave.ExtensionMethods;

using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Identity;

public static class UserExtensions
{
    public static async Task SeedUserRoles(this UserManager<User> userManager, params string[] emails)
    {
        foreach (var email in emails)
        {
            await userManager.EnsureUserHasRole(email.Trim(), Roles.ADMIN);
        }
    }

    public static async Task SeedRoles(this RoleManager<MemberRole> roleManager)
    {
        foreach (var role in Roles.All)
        {
            await roleManager.CreateRoleIfMissing(role);
        }
    }

    private static async Task CreateRoleIfMissing(this RoleManager<MemberRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new MemberRole
            {
                Name = role
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

    public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
        => roles.Any(user.IsInRole);

    public static string GetFullName(this ClaimsPrincipal user) => user.FindFirstValue("FullName") ?? user.GetEmail();

    public static string GetFriendlyName(this ClaimsPrincipal user) => user.FindFirstValue("FriendlyName")?.ToNullIfEmpty() ?? user.GetFullName()?.Split(' ').FirstOrDefault() ?? "danser";

    public static string GetFriendlyName(this User user) => user.FriendlyName?.ToNullIfEmpty() ?? user.FullName?.Split(' ').FirstOrDefault() ?? string.Empty;

    public static string GetEmail(this ClaimsPrincipal user) => user.Identity.Name;

    public static string GetId(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier);
}
