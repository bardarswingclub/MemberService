using MemberService.Data;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public interface ILoginService
    {
        Task<MemberUser> GetOrCreateUser(string email);
        bool IsLoggedIn(System.Security.Claims.ClaimsPrincipal user);
        Task<string> LoginCode(MemberUser user);
        Task<string> LoginLink(MemberUser user, string returnUrl);
    }
}
