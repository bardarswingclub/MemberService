namespace MemberService.Services;


using MemberService.Data;

using Microsoft.AspNetCore.Authentication;

public interface ILoginService
{
    Task<User> GetOrCreateUser(string email);
    bool IsLoggedIn(System.Security.Claims.ClaimsPrincipal user);
    Task<string> LoginCode(User user);
    Task<string> LoginLink(User user, string returnUrl);
    Task<IList<AuthenticationScheme>> GetExternalAuthenticationSchemes();
}
