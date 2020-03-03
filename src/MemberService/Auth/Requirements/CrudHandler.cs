using System.Linq;
using System.Threading.Tasks;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace MemberService.Auth.Requirements
{
    public class CrudHandler : AuthorizationHandler<CrudHandler.CrudRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CrudHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CrudRequirement requirement)
        {
            if (CanPerformOperation())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool CanPerformOperation()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            string method = httpContext.Request.Method;
            var user = httpContext.User;

            if (HttpMethods.IsPost(method))
            {
                return user.IsInRole(Roles.Coordinator);
            }

            if (HttpMethods.IsGet(method))
            {
                return user.IsInAnyRole(Roles.Coordinator, Roles.Instructor);
            }

            return false;
        }

        public class CrudRequirement : IAuthorizationRequirement { }
    }
}