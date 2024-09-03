namespace MemberService.Auth.Requirements;
using MemberService.Data;

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
            Policy.CanToggleUserFeeExemption => user.IsInAnyRole(R.STYRET),

            Policy.CanCreateEvent => user.IsInAnyRole(R.FESTKOM, R.STYRET, R.WORKSHOPADM),

            Policy.CanListEvents => user.IsInAnyRole(R.STYRET, R.FESTKOM, R.WORKSHOPADM),
            Policy.CanViewEvent => user.IsInAnyRole(R.STYRET),
            Policy.CanEditEventOrganizers => user.IsInAnyRole(R.STYRET),

            Policy.CanCreateSurvey => user.IsInAnyRole(R.STYRET),
            Policy.CanViewSurvey => user.IsInAnyRole(R.STYRET),
            Policy.CanEditSurvey => user.IsInAnyRole(R.STYRET),

            Policy.CanViewMembers => user.IsInAnyRole(R.STYRET, R.FESTKOM),
            Policy.CanViewOlderMembers => user.IsInAnyRole(R.STYRET),
            Policy.CanSendEmailToMembers => user.IsInAnyRole(R.STYRET),
            Policy.CanAddManualPayment => user.IsInAnyRole(R.STYRET),
            Policy.CanUpdatePayments => user.IsInAnyRole(R.STYRET),
            Policy.CanSeeStripeLink => user.IsInAnyRole(R.STYRET),

            Policy.CanViewSemester => user.IsInAnyRole(R.STYRET),
            Policy.CanCreateSemester => user.IsInAnyRole(R.STYRET),
            Policy.CanCreateSemesterEvent => user.IsInAnyRole(R.STYRET),
            Policy.CanEditSemester => user.IsInAnyRole(R.STYRET),
            Policy.CanEditSemesterRoles => user.IsInAnyRole(R.STYRET),
            Policy.CanPreviewSemesterSignup => user.IsInAnyRole(R.STYRET),

            Policy.CanCreateAnnualMeeting => user.IsInAnyRole(R.STYRET),
            Policy.CanEditAnnualMeeting => user.IsInAnyRole(R.STYRET),
            Policy.CanViewAnnualMeetingAttendees => user.IsInAnyRole(R.STYRET),

            Policy.CanListPayments => user.IsInAnyRole(R.STYRET),

            _ => false,
        };
}
