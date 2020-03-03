using MemberService.Auth.Requirements;
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
                .AddSingleton<IAuthorizationHandler, CrudHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(Policy.IsAdmin), Policy.IsAdmin);

                options.AddPolicy(nameof(Policy.IsCoordinator), Policy.IsCoordinator);

                options.AddPolicy(nameof(Policy.IsInstructor), Policy.IsInstructor);

                options.AddPolicy(nameof(Policy.CanListMembers), Policy.CanListMembers);

                options.AddPolicy(nameof(Policy.CrudRoute), Policy.CrudRoute);
            });

            services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory>();
        }
    }
}