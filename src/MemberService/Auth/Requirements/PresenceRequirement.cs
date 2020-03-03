using System.Threading.Tasks;
using MemberService.Data.ValueTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace MemberService.Auth.Requirements
{
    public class PresenceRequirement : IAuthorizationRequirement
    {

    }

    public static class Operations
    {
        public static OperationAuthorizationRequirement List = new OperationAuthorizationRequirement { Name = nameof(List) };
        public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = nameof(Create) };
        public static OperationAuthorizationRequirement View = new OperationAuthorizationRequirement { Name = nameof(View) };
        public static OperationAuthorizationRequirement Modify = new OperationAuthorizationRequirement { Name = nameof(Modify) };
        public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = nameof(Delete) };
        public static OperationAuthorizationRequirement RecordPresence = new OperationAuthorizationRequirement { Name = nameof(RecordPresence) };
        public static OperationAuthorizationRequirement GrantAccess = new OperationAuthorizationRequirement { Name = nameof(GrantAccess) };
    }

    public class DocumentAuthorizationCrudHandler :
        AuthorizationHandler<OperationAuthorizationRequirement, EventType>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            EventType resource)
        {
            /*if (context.User.Identity?.Name == resource.Author &&
                requirement.Name == Operations.Read.Name)
            {
                context.Succeed(requirement);
            }*/

            return Task.CompletedTask;
        }
    }
}