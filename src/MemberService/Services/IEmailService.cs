using MemberService.Data;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public interface IEmailService
    {
        Task SendLoginEmail(string email, Emails.Account.LoginModel model);

        Task<bool> SendEventStatusEmail(string email, Status status, Emails.Event.EventStatusModel model);
    }
}