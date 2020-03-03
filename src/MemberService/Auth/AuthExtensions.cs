using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MemberService.Auth
{
    public static class AuthExtensions
    {
        public static void AddAuthorizationRules(this IServiceCollection services)
        {
            services
                .AddSingleton<IAuthorizationHandler, AdminHandler>()
                .AddScoped<IAuthorizationHandler, Permission.Handler>();

            services.AddAuthorization(Permission.AddPolicies);

            services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();
        }
    }
}