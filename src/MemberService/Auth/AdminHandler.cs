using System.Linq;
using System.Threading.Tasks;
using MemberService.Data.ValueTypes;
using Microsoft.AspNetCore.Authorization;

namespace MemberService.Auth
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