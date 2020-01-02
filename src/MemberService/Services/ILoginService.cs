using MemberService.Data;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public interface ILoginService
    {
        Task<User> GetOrCreateUser(string email);
        bool IsLoggedIn(System.Security.Claims.ClaimsPrincipal user);
        Task<string> LoginCode(User user);
        Task<string> LoginLink(User user, string returnUrl);
    }
}
