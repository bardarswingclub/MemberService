using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

using MemberService.Auth.Requirements;
using MemberService.Data.ValueTypes;
using Microsoft.AspNetCore.Authorization;

namespace MemberService.Auth
{
    public static class Policy
    {
        public static void IsAdmin(AuthorizationPolicyBuilder policy) =>
            policy.Requirements.Add(IsInRole.Admin);

        public static void IsCoordinator(AuthorizationPolicyBuilder policy) =>
            policy.Requirements.Add(IsInRole.Coordinator);

        public static void IsInstructor(AuthorizationPolicyBuilder policy) =>
            policy.Requirements.Add(IsInRole.Instructor);

        public static IEnumerable<(string, Action<AuthorizationPolicyBuilder>)> Policies
            => typeof(Policy)
            .GetMethods()
            .Where(m => m.ReturnType == typeof(void))
            .Where(m => m.GetParameters().Length == 1)
            .Select(p => (p.Name, p.CreateDelegate<Action<AuthorizationPolicyBuilder>>()));
    }
}
