using MemberService.Data;
using Microsoft.AspNetCore.Authorization;

namespace MemberService.Auth
{
    public static class Policy
    {
        public static void IsAdmin(AuthorizationPolicyBuilder policy) =>
            policy.Requirements.Add(new OneOfAnyRoleRequirement(Roles.ADMIN));

        public static void IsCoordinator(AuthorizationPolicyBuilder policy) =>
            policy.Requirements.Add(new OneOfAnyRoleRequirement(Roles.COORDINATOR, Roles.ADMIN));

        public static void IsInstructor(AuthorizationPolicyBuilder policy) =>
            policy.Requirements.Add(new OneOfAnyRoleRequirement(Roles.INSTRUCTOR, Roles.COORDINATOR, Roles.ADMIN));
    }
}
