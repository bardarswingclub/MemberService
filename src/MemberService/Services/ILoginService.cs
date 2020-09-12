using System.Collections;
using System.Collections.Generic;
using MemberService.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace MemberService.Services
{
    public interface ILoginService
    {
        Task<User> GetOrCreateUser(string email);
        bool IsLoggedIn(System.Security.Claims.ClaimsPrincipal user);
        Task<string> LoginCode(User user);
        Task<string> LoginLink(User user, string returnUrl);
        Task<IList<AuthenticationScheme>> GetExternalAuthenticationSchemes();
    }
}
