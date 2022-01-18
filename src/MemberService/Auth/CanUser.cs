namespace MemberService.Auth;

using System.Security.Claims;

using MemberService.Data;
using MemberService.Data.ValueTypes;

public static class CanUser
{
    public static bool IsAdministrator(this ClaimsPrincipal user) => user.IsInRole(Roles.ADMIN);


    public static bool CanCreateWorkshop(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.STYRET);

    public static bool CanCreateParty(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.FESTKOM);
}
