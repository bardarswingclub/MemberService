using System;
using System.Collections.Generic;
using System.Linq;

using MemberService.Data.ValueTypes;
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

        public static void CanToggleRole(AuthorizationPolicyBuilder policy) =>
            policy.Requirements.Add(new CanToggleRoleRequirement());

        public static IEnumerable<(string, Action<AuthorizationPolicyBuilder>)> Policies
            => typeof(Policy)
            .GetMethods()
            .Where(m => m.ReturnType == typeof(void))
            .Where(m => m.GetParameters().Length == 1)
            .Select(p => (p.Name, p.CreateDelegate<Action<AuthorizationPolicyBuilder>>()));
    }
}
