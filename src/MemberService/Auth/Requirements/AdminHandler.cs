using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MemberService.Data.ValueTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MemberService.Auth.Requirements
{
    public class AdminHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (!context.User.IsInRole(Roles.Admin))
            {
                return Task.CompletedTask;
            }

            foreach (var requirement in context.PendingRequirements.ToList())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}