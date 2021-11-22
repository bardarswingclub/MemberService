using System.Threading.Tasks;

using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;

namespace MemberService.Auth.Requirements
{
    public class IsInRole : AuthorizationHandler<IsInRole>, IAuthorizationRequirement
    {
        private readonly string[] _roles;

        public static readonly IsInRole Admin = new(Roles.ADMIN);

        public static readonly IsInRole Coordinator = new(Roles.COORDINATOR, Roles.ADMIN);

        public static readonly IsInRole Instructor = new(Roles.INSTRUCTOR, Roles.COORDINATOR, Roles.ADMIN);

        public IsInRole(params string[] roles)
        {
            _roles = roles;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsInRole requirement)
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
