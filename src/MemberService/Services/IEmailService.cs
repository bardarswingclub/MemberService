using MemberService.Data;
using MemberService.Emails.Event;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public interface IEmailService
    {
        Task SendLoginEmail(string email, Emails.Account.LoginModel model);

        Task<bool> SendEventStatusEmail(string email, Status status, Emails.Event.EventStatusModel model);

        Task SendCustomEmail(string email, string subject, string message, EventStatusModel eventStatusModel);
    }
}