using System.Threading.Tasks;

using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;

namespace MemberService.Auth
{
    public class CanToggleRoleRequirement : AuthorizationHandler<CanToggleRoleRequirement, string>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanToggleRoleRequirement requirement,
            string resource)
        {
            if (context.User.IsInRole(Roles.ADMIN))
            {
                context.Succeed(requirement);
            }

            if (context.User.IsInRole(Roles.COORDINATOR) && resource == Roles.INSTRUCTOR)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}