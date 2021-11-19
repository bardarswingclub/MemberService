using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using MemberService.Data;
using MemberService.Data.ValueTypes;

namespace MemberService.Auth
{
    public static class CanUser
    {
        public static bool IsAdministrator(this ClaimsPrincipal user) => user.IsInRole(Roles.ADMIN);

        public static bool CanEditSignup(this ClaimsPrincipal user) => user.IsInRole(Roles.ADMIN);

        public static bool CanEditEvent(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanEditSemester(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanViewSignupHistory(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanCreateEvent(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanSetSignupStatus(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanPreviewSignup(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanCreateCourse(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanViewMembers(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR);

        public static bool CanViewSemester(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR);

        public static bool CanCreateSemester(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR);

        public static bool CanViewEvents(this ClaimsPrincipal user) => user.IsInAnyRole(Roles.ADMIN, Roles.COORDINATOR, Roles.INSTRUCTOR);
    }
}
