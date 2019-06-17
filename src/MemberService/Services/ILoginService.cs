using MemberService.Data;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public interface ILoginService
    {
        Task<MemberUser> GetOrCreateUser(string email);

        Task<string> LoginLink(MemberUser user, string returnUrl);
    }
}
