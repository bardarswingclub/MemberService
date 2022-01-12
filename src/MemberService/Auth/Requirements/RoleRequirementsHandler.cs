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
            if (IsAuthorized(user, requirement))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsAuthorized(System.Security.Claims.ClaimsPrincipal user, Requirement requirement)
        => requirement.Policy switch
        {
            Policy.IsInstructor => user.IsInAnyRole(R.INSTRUCTOR, R.COORDINATOR, R.ADMIN),
            Policy.IsCoordinator => user.IsInAnyRole(R.COORDINATOR, R.ADMIN),
            Policy.IsAdmin => user.IsInAnyRole(R.ADMIN),

            Policy.CanCreateEvent => user.IsInAnyRole(R.FESTKOM, R.COORDINATOR, R.ADMIN),

            Policy.CanListEvents => user.IsInAnyRole(R.ADMIN, R.COORDINATOR, R.INSTRUCTOR, R.FESTKOM),
            Policy.CanViewEvent => user.IsInAnyRole(R.ADMIN, R.COORDINATOR, R.INSTRUCTOR),
            Policy.CanEditEvent => user.IsInAnyRole(R.ADMIN, R.COORDINATOR),
            Policy.CanSetEventSignupStatus => user.IsInAnyRole(R.ADMIN, R.COORDINATOR),
            Policy.CanEditEventSignup => user.IsInAnyRole(R.ADMIN),
            Policy.CanEditEventOrganizers => user.IsInAnyRole(R.ADMIN),

            Policy.CanSetPresence => user.IsInAnyRole(R.ADMIN, R.COORDINATOR, R.INSTRUCTOR),
            Policy.CanAddPresenceLesson => user.IsInAnyRole(R.ADMIN, R.COORDINATOR, R.INSTRUCTOR),

            Policy.CanCreateSurvey => user.IsInAnyRole(R.ADMIN, R.COORDINATOR),
            Policy.CanViewSurvey => user.IsInAnyRole(R.ADMIN, R.COORDINATOR, R.INSTRUCTOR),
            Policy.CanEditSurvey => user.IsInAnyRole(R.ADMIN, R.COORDINATOR),

            Policy.CanViewMembers => user.IsInAnyRole(R.ADMIN, R.COORDINATOR, R.INSTRUCTOR, R.FESTKOM),

            Policy.CanViewSemester => user.IsInAnyRole(R.ADMIN, R.COORDINATOR, R.INSTRUCTOR),
            Policy.CanCreateSemester => user.IsInAnyRole(R.ADMIN, R.COORDINATOR),
            Policy.CanEditSemester => user.IsInAnyRole(R.ADMIN),
            Policy.CanEditSemesterRoles => user.IsInAnyRole(R.ADMIN),
            Policy.CanPreviewSemesterSignup => user.IsInAnyRole(R.ADMIN),

            _ => false,
        };
}
