namespace MemberService.Auth;

using System.Security.Claims;

using MemberService.Data;
using MemberService.Data.ValueTypes;

public static class CanUser
{
    public static bool IsAdministrator(this ClaimsPrincipal user) => user.IsInRole(Roles.ADMIN);


    public static bool CanCreateWorkshop(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

    public static bool CanCreateParty(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.FESTKOM);


    public static bool CanEditSemester(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

    public static bool CanPreviewSignup(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

    public static bool CanToggleRole(this ClaimsPrincipal user, string role = null)
        => user.CanToggleAllRoles()
        || role == Roles.INSTRUCTOR && user.CanToggleInstructorRole();

    public static bool CanToggleAllRoles(this ClaimsPrincipal user) => user.IsInRole(Roles.ADMIN);

    public static bool CanToggleInstructorRole(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.COORDINATOR, Roles.ADMIN);
}
