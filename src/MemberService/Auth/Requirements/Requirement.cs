namespace MemberService.Auth.Requirements;

using Microsoft.AspNetCore.Authorization;

public record Requirement(Policy Policy) : IAuthorizationRequirement;
