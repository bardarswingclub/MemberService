using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace MemberService.Auth
{
    public class Permission
    {
        public enum Resource
        {
            Member,
            Event,
            Signup,
            Survey,
            Semester,
            Presence
        }

        public enum Action
        {
            List,
            View,
            Edit,
            Manage,
            Create
        }

        public static void AddPolicies(AuthorizationOptions options)
        {
            var actions = Enum.GetValues(typeof(Action)).Cast<Action>();
            var resources = Enum.GetValues(typeof(Resource)).Cast<Resource>().ToList();
            foreach (var action in actions)
            {
                foreach (var resource in resources)
                {
                    options.AddPolicy(PolicyName(action, resource), p => p.AddRequirements(new Requirement(action, resource)));
                }
            }
        }

        private static string PolicyName(Action action, Resource resource) => $"PermissionTo-{action}-{resource}";

        public class ToAttribute : AuthorizeAttribute
        {
            public ToAttribute(Action action, Resource resource) : base(PolicyName(action, resource)) { }
        }

        public class Handler : AuthorizationHandler<Requirement >
        {
            private readonly IHttpContextAccessor _httpContextAccessor;

            public Handler(IHttpContextAccessor httpContextAccessor)
            {
                _httpContextAccessor = httpContextAccessor;
            }

            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, Requirement requirement)
            {
                if (CanPerformOperation(requirement, context.User))
                {
                    context.Succeed(requirement);
                }

                return Task.CompletedTask;
            }

            private bool CanPerformOperation(Requirement requirement, IPrincipal user)
            {
                return requirement switch
                {
                    (Resource.Event, Action.List) => user.IsInAnyRole(Roles.Instructor, Roles.Coordinator, Roles.FestKom),
                    (Resource.Event, Action.View) => user.IsInAnyRole(Roles.Instructor, Roles.Coordinator, Roles.FestKom),
                    (Resource.Event, Action.Manage) => user.IsInAnyRole(Roles.Coordinator, Roles.FestKom),
                    (Resource.Event, Action.Edit) => user.IsInAnyRole(Roles.Coordinator, Roles.FestKom),
                    (Resource.Event, Action.Create) => user.IsInAnyRole(Roles.Coordinator, Roles.FestKom),

                    (Resource.Signup, Action.Edit) => user.IsAdmin(),

                    (Resource.Member, Action.List) => user.IsInAnyRole(Roles.Instructor, Roles.Coordinator, Roles.FestKom),
                    (Resource.Member, Action.View) => user.IsInAnyRole(Roles.Instructor, Roles.Coordinator, Roles.FestKom),

                    (Resource.Survey, Action.View) => user.IsInAnyRole(Roles.Instructor, Roles.Coordinator, Roles.FestKom),
                    (Resource.Survey, Action.Edit) => user.IsInAnyRole(Roles.Coordinator, Roles.FestKom),

                    (Resource.Semester, Action.View) => user.IsInAnyRole(Roles.Instructor, Roles.Coordinator, Roles.FestKom),
                    (Resource.Semester, Action.Create) => user.IsInAnyRole(Roles.Coordinator, Roles.FestKom),
                    (Resource.Semester, Action.Edit) => user.IsInAnyRole(Roles.Coordinator, Roles.FestKom),

                    (Resource.Presence, Action.View) => user.IsInAnyRole(Roles.Instructor, Roles.Coordinator, Roles.FestKom),
                    (Resource.Presence, Action.Edit) => user.IsInAnyRole(Roles.Instructor, Roles.Coordinator, Roles.FestKom),
                    _ => false
                };
            }
        }

        public class Requirement : IAuthorizationRequirement
        {
            public Action Action { get; }

            public Resource Resource { get; }

            public Requirement(Action action, Resource resource)
            {
                Action = action;
                Resource = resource;
            }

            public void Deconstruct(out Resource resource, out Action action)
            {
                action = Action;
                resource = Resource;
            }
        }
    }
}