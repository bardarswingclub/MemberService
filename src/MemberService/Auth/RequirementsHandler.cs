namespace MemberService.Auth
{
    using System;
    using System.Linq;
    using System.Security.Claims;
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
            var id = GetGuid(resource as HttpContext);

            foreach (var requirement in pendingRequirements)
            {
                var authorized = requirement.Policy switch
                {
                    Policy.IsInstructor => user.IsInAnyRole(Roles.INSTRUCTOR, Roles.COORDINATOR, Roles.ADMIN),
                    Policy.IsCoordinator => user.IsInAnyRole(Roles.COORDINATOR, Roles.ADMIN),
                    Policy.IsAdmin => user.IsInAnyRole(Roles.ADMIN),

                    Policy.CanCreateEvent => user.IsInAnyRole(Roles.FESTKOM, Roles.COORDINATOR, Roles.ADMIN),

                    Policy.CanListEvents => await CanListEvents(context),
                    Policy.CanViewEvent when id is Guid eventId => await CanViewEvent(context, eventId),
                    Policy.CanEditEvent when (id ?? resource) is Guid eventId => await CanEditEvent(context, eventId),
                    Policy.CanSetEventSignupStatus when (id ?? resource) is Guid eventId => await CanSetEventSignupStatus(context, eventId),
                    Policy.CanEditEventSignup when (id ?? resource) is Guid eventId => await CanEditEventSignup(context, eventId),
                    Policy.CanEditEventOrganizers when (id ?? resource) is Guid eventId => await CanEditEventOrganizers(context, eventId),

                    Policy.CanSetPresence when (id ?? resource) is Guid eventId => await CanSetPresence(context, eventId),
                    Policy.CanAddPresenceLesson when (id ?? resource) is Guid eventId => await CanAddPresenceLesson(context, eventId),

                    Policy.CanCreateSurvey when (id ?? resource) is Guid eventId => await CanEditEvent(context, eventId),
                    Policy.CanViewSurvey when (id ?? resource) is Guid eventId => await CanViewEvent(context, eventId),
                    Policy.CanEditSurvey when (id ?? resource) is Guid eventId => await CanEditEvent(context, eventId),

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
            if (context.User.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR, Roles.FESTKOM))
            {
                return true;
            }

            var userId = _userManager.GetUserId(context.User);
            return await _database.EventOrganizers
                .AnyAsync(o => o.UserId == userId && !o.Event.Archived);
            
        }

        private async Task<bool> CanViewEvent(AuthorizationHandlerContext context, Guid eventId)
        {
            if (context.User.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR))
            {
                return true;
            }

            return await CheckEventOrganizer(eventId, context.User, _ => true);
        }

        private async Task<bool> CanEditEvent(AuthorizationHandlerContext context, Guid eventId)
        {
            if (context.User.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR))
            {
                return true;
            }

            return await CheckEventOrganizer(eventId, context.User, p => p.CanEdit);
        }

        private async Task<bool> CanSetEventSignupStatus(AuthorizationHandlerContext context, Guid eventId)
        {
            if (context.User.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR))
            {
                return true;
            }

            return await CheckEventOrganizer(eventId, context.User, p => p.CanSetSignupStatus);
        }

        private async Task<bool> CanEditEventSignup(AuthorizationHandlerContext context, Guid eventId)
        {
            if (context.User.IsInAnyRole(Roles.ADMIN))
            {
                return true;
            }

            return await CheckEventOrganizer(eventId, context.User, p => p.CanEditSignup);
        }

        private async Task<bool> CanEditEventOrganizers(AuthorizationHandlerContext context, Guid eventId)
        {
            if (context.User.IsInAnyRole(Roles.ADMIN))
            {
                return true;
            }

            return await CheckEventOrganizer(eventId, context.User, p => p.CanEditOrganizers);
        }

        private async Task<bool> CanSetPresence(AuthorizationHandlerContext context, Guid eventId)
        {
            if (context.User.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR))
            {
                return true;
            }

            return await CheckEventOrganizer(eventId, context.User, p => p.CanSetPresence);
        }

        private async Task<bool> CanAddPresenceLesson(AuthorizationHandlerContext context, Guid eventId)
        {
            if (context.User.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR))
            {
                return true;
            }

            return await CheckEventOrganizer(eventId, context.User, p => p.CanAddPresenceLesson);
        }

        private async Task<bool> CheckEventOrganizer(Guid eventId, ClaimsPrincipal user, Func<EventOrganizer, bool> check)
        {
            var userId = _userManager.GetUserId(user);
            var permissions = await _database.EventOrganizers.FindAsync(eventId, userId);

            if (permissions is null) return false;

            return check(permissions);
        }

        private static Guid? GetGuid(HttpContext httpContext) => httpContext?.GetRouteValue("id") is string x && Guid.TryParse(x, out var y) ? y : (Guid?)default;
    }
}
