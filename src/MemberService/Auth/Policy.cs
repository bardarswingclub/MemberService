using MemberService.Auth.Requirements;
using MemberService.Data.ValueTypes;
using Microsoft.AspNetCore.Authorization;

namespace MemberService.Auth
{
    public static class Policy
    {
        public static void IsAdmin(AuthorizationPolicyBuilder policy) =>
            policy.RequireRole(Roles.Admin);

        public static void IsCoordinator(AuthorizationPolicyBuilder policy) =>
            policy.RequireRole(Roles.Coordinator);

        public static void IsInstructor(AuthorizationPolicyBuilder policy) =>
            policy.RequireRole(Roles.Instructor, Roles.Coordinator);

        public static void CanListMembers(AuthorizationPolicyBuilder policy) =>
            policy.RequireRole(Roles.Instructor, Roles.Coordinator);

        public static void CrudRoute(AuthorizationPolicyBuilder policy) =>
            policy.AddRequirements(new CrudHandler.CrudRequirement());

        public static string ListWorkshops = nameof(ListWorkshops);

        public static string CreateWorkshop = nameof(CreateWorkshop);

        public static string ListPartys = nameof(ListPartys);

        public static string CreateParty = nameof(CreateParty);
    }
}
