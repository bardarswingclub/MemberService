namespace MemberService.Auth.Requirements;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;

using R = Data.ValueTypes.Roles;

public class RoleRequirementsHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.OfType<Requirement>().ToList();
        var user = context.User;

        if (!user.Identity.IsAuthenticated) return Task.CompletedTask;

        foreach (var requirement in pendingRequirements)
        {
            if (user.IsInRole(R.ADMIN))
            {
                context.Succeed(requirement);
            }
            else if (IsAuthorized(user, requirement))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsAuthorized(System.Security.Claims.ClaimsPrincipal user, Requirement requirement)
        => requirement.Policy switch
        {
            Policy.IsAdmin => user.IsInAnyRole(R.ADMIN),

            Policy.CanToggleRoles => user.IsInAnyRole(R.ADMIN),

            Policy.CanCreateEvent => user.IsInAnyRole(R.FESTKOM, R.COORDINATOR),

            Policy.CanListEvents => user.IsInAnyRole(R.COORDINATOR, R.INSTRUCTOR, R.FESTKOM),
            Policy.CanViewEvent => user.IsInAnyRole(R.COORDINATOR, R.INSTRUCTOR),
            Policy.CanEditEvent => user.IsInAnyRole(R.COORDINATOR),
            Policy.CanSetEventSignupStatus => user.IsInAnyRole(R.COORDINATOR),
            Policy.CanEditEventOrganizers => user.IsInAnyRole(R.STYRET),

            Policy.CanSetPresence => user.IsInAnyRole(R.COORDINATOR, R.INSTRUCTOR),
            Policy.CanAddPresenceLesson => user.IsInAnyRole(R.COORDINATOR, R.INSTRUCTOR),

            Policy.CanCreateSurvey => user.IsInAnyRole(R.COORDINATOR),
            Policy.CanViewSurvey => user.IsInAnyRole(R.COORDINATOR, R.INSTRUCTOR),
            Policy.CanEditSurvey => user.IsInAnyRole(R.COORDINATOR),

            Policy.CanViewMembers => user.IsInAnyRole(R.COORDINATOR, R.INSTRUCTOR, R.FESTKOM),

            Policy.CanViewSemester => user.IsInAnyRole(R.STYRET),
            Policy.CanCreateSemester => user.IsInAnyRole(R.STYRET),
            Policy.CanEditSemester => user.IsInAnyRole(R.STYRET),
            Policy.CanEditSemesterRoles => user.IsInAnyRole(R.STYRET),
            Policy.CanPreviewSemesterSignup => user.IsInAnyRole(R.STYRET),

            _ => false,
        };
}
