using MemberService.Emails.Event;
using System.Threading.Tasks;
using MemberService.Data;

namespace MemberService.Services
{
    public interface IEmailService
    {
        Task SendLoginEmail(string email, string name, Emails.Account.LoginModel model);

        Task SendCustomEmail(User to, string subject, string message, EventStatusModel eventStatusModel, User replyTo = null);
    }
}