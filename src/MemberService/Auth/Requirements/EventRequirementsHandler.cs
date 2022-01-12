namespace MemberService.Auth.Requirements;

using System.Security.Claims;

using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

public class EventRequirementsHandler : IAuthorizationHandler
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;

    public EventRequirementsHandler(MemberContext database, UserManager<User> userManager)
    {
        _database = database;
        _userManager = userManager;
    }

    public async Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.OfType<Requirement>().ToList();
        var user = context.User;
        
        if (!user.Identity.IsAuthenticated) return;

        var id = context.GetGuidResource();

        foreach (var requirement in pendingRequirements)
        {
            if (await IsAuthorized(user, id, requirement))
            {
                context.Succeed(requirement);
            }
        }
    }

    private async Task<bool> IsAuthorized(ClaimsPrincipal user, Guid? id, Requirement requirement)
        => requirement.Policy switch
        {
            Policy.CanListEvents => await CanListEvents(user),
            Policy.CanViewEvent when id is Guid eventId => await CheckEventOrganizer(eventId, user, _ => true),
            Policy.CanEditEvent when id is Guid eventId => await CheckEventOrganizer(eventId, user, p => p.CanEdit),
            Policy.CanSetEventSignupStatus when id is Guid eventId => await CheckEventOrganizer(eventId, user, p => p.CanSetSignupStatus),
            Policy.CanEditEventSignup when id is Guid eventId => await CheckEventOrganizer(eventId, user, p => p.CanEditSignup),
            Policy.CanEditEventOrganizers when id is Guid eventId => await CheckEventOrganizer(eventId, user, p => p.CanEditOrganizers),

            Policy.CanSetPresence when id is Guid eventId => await CheckEventOrganizer(eventId, user, p => p.CanSetPresence),
            Policy.CanAddPresenceLesson when id is Guid eventId => await CheckEventOrganizer(eventId, user, p => p.CanAddPresenceLesson),

            Policy.CanCreateSurvey when id is Guid eventId => await CheckEventOrganizer(eventId, user, p => p.CanEdit),
            Policy.CanViewSurvey when id is Guid eventId => await CheckEventOrganizer(eventId, user, _ => true),
            Policy.CanEditSurvey when id is Guid eventId => await CheckEventOrganizer(eventId, user, p => p.CanEdit),

            Policy.CanViewMembers => await CanListEvents(user),
            _ => false,
        };

    private async Task<bool> CanListEvents(ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);
        return await _database.EventOrganizers
            .AnyAsync(o => o.UserId == userId && !o.Event.Archived);

    }

    private async Task<bool> CheckEventOrganizer(Guid eventId, ClaimsPrincipal user, Func<EventOrganizer, bool> check)
    {
        var userId = _userManager.GetUserId(user);
        var permissions = await _database.EventOrganizers.FindAsync(eventId, userId);

        if (permissions is null) return false;

        return check(permissions);
    }
}
