using System.Security.Claims;

using MemberService.Data;
using MemberService.Data.ValueTypes;

namespace MemberService.Auth
{
    public static class CanUser
    {
        public static bool CanCreateEvent(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanSetSignupStatus(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanPreviewSignup(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanEditEventOrganizers(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanViewSignupHistory(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanEditSignup(this ClaimsPrincipal user) => user.IsInRole(Roles.ADMIN);


        public static bool CanViewPresence(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR);

        public static bool CanSetPresence(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR);

        public static bool CanAddPresenceLesson(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR);


        public static bool IsAdministrator(this ClaimsPrincipal user) => user.IsInRole(Roles.ADMIN);

        public static bool CanEditSemester(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanCreateCourse(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanViewMembers(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR);

        public static bool CanViewSemester(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR);

        public static bool CanCreateSemester(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanToggleRole(this ClaimsPrincipal user, string role = null)
            => user.CanToggleAllRoles()
            || role == Roles.INSTRUCTOR && user.CanToggleInstructorRole();

        public static bool CanToggleAllRoles(this ClaimsPrincipal user) => user.IsInRole(Roles.ADMIN);

        public static bool CanToggleInstructorRole(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.COORDINATOR, Roles.ADMIN);

        public static bool CanCreateSurvey(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.COORDINATOR, Roles.ADMIN);

        public static bool CanEditSurvey(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.COORDINATOR, Roles.ADMIN);
    }
}
