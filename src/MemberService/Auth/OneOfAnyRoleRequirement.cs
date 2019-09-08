using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;

namespace MemberService.Auth
{
    public class OneOfAnyRoleRequirement : AuthorizationHandler<OneOfAnyRoleRequirement>, IAuthorizationRequirement
    {
        private readonly string[] _roles;

        public OneOfAnyRoleRequirement(params string[] roles)
        {
            _roles = roles;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OneOfAnyRoleRequirement requirement)
        {
            if (context.User.IsInAnyRole(_roles))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}
