using MemberService.Emails.Event;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public interface IEmailService
    {
        Task SendLoginEmail(string email, Emails.Account.LoginModel model);

        Task SendCustomEmail(string email, string subject, string message, EventStatusModel eventStatusModel);
    }
}