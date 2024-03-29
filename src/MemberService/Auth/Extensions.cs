namespace MemberService.Auth;

using System.Security.Claims;

using MemberService.Auth.Development;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

public static class Extensions
{
    public static IdentityBuilder AddPasswordlessLoginTokenProvider(this IdentityBuilder builder, bool isDevelopment)
    {
        builder.AddTokenProvider(
            "LongToken",
            typeof(LongTokenProvider<>).MakeGenericType(builder.UserType));

        builder.AddTokenProvider(
            "ShortToken",
            GetShortTokenProvider(isDevelopment).MakeGenericType(builder.UserType));

        return builder;
    }

    public static async Task<bool> IsAuthorized(this IAuthorizationService service, ClaimsPrincipal user, Policy policy)
    {
        var result = await service.AuthorizeAsync(user, null, policy.ToString());
        return result.Succeeded;
    }

    public static async Task<bool> IsAuthorized(this IAuthorizationService service, ClaimsPrincipal user, object resource, Policy policy)
    {
        var result = await service.AuthorizeAsync(user, resource, policy.ToString());
        return result.Succeeded;
    }

    public static async Task<bool> IsAuthorized(this IAuthorizationService service, ClaimsPrincipal user, string policy)
    {
        var result = await service.AuthorizeAsync(user, null, policy);
        return result.Succeeded;
    }

    public static async Task<bool> IsAuthorized(this IAuthorizationService service, ClaimsPrincipal user, string policy, object resource)
    {
        var result = await service.AuthorizeAsync(user, resource, policy);
        return result.Succeeded;
    }

    private static System.Type GetShortTokenProvider(bool useDummyProvider)
    {
        return useDummyProvider ? typeof(DummyShortTokenProvider<>) : typeof(ShortTokenProvider<>);
    }

    public static Guid? GetGuidResource(this AuthorizationHandlerContext context)
    {
        return context.Resource as Guid? ?? ((context.Resource as HttpContext)?.GetRouteValue("id") is string x && Guid.TryParse(x, out var y) ? y : (Guid?)default);
    }
}
