namespace MemberService.Auth
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using MemberService.Auth.Requirements;
    using MemberService.Data;
    using MemberService.Data.ValueTypes;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;

    public class RequirementsHandler : IAuthorizationHandler
    {
        private readonly MemberContext _database;
        private readonly UserManager<User> _userManager;

        public RequirementsHandler(MemberContext database, UserManager<User> userManager)
        {
            _database = database;
            _userManager = userManager;
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var pendingRequirements = context.PendingRequirements.OfType<Requirement>().ToList();
            var user = context.User;
            var resource = context.Resource;

            foreach (var requirement in pendingRequirements)
            {
                var authorized = requirement.Policy switch
                {
                    Policy.IsInstructor => user.IsInAnyRole(Roles.INSTRUCTOR, Roles.COORDINATOR, Roles.ADMIN),
                    Policy.IsCoordinator => user.IsInAnyRole(Roles.COORDINATOR, Roles.ADMIN),
                    Policy.IsAdmin => user.IsInAnyRole(Roles.ADMIN),

                    Policy.CanListEvents => await CanListEvents(context),
                    Policy.CanViewEvent 
                        when resource is HttpContext httpContext 
                        && httpContext.GetRouteValue("id") is string id
                        && Guid.TryParse(id, out var eventId) 
                        => await CanViewEvent(context, eventId),

                    Policy.CanViewMembers => await CanListEvents(context),
                };

                if (authorized)
                {
                    context.Succeed(requirement);
                }
            }
        }

        private async Task<bool> CanListEvents(AuthorizationHandlerContext context)
        {
            if (context.User.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR))
            {
                return true;
            }

            var userId = _userManager.GetUserId(context.User);
            var isOrganizingAnything = await _database.EventOrganizers.AnyAsync(o => o.UserId == userId);
            if (isOrganizingAnything)
            {
                return true;
            }

            return false;
        }

        private async Task<bool> CanViewEvent(AuthorizationHandlerContext context, Guid eventId)
        {
            if (context.User.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR))
            {
                return true;
            }

            var userId = _userManager.GetUserId(context.User);
            var isOrganizingEvent = await _database.EventOrganizers.AnyAsync(o => o.UserId == userId && o.EventId == eventId);
            if (isOrganizingEvent)
            {
                return true;
            }

            return false;
        }
    }
}
