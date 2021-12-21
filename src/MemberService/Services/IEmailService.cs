namespace MemberService.Services;

using MemberService.Emails.Event;

using MemberService.Data;

public interface IEmailService
{
    Task SendLoginEmail(string email, string name, Emails.Account.LoginModel model);

    Task SendCustomEmail(User to, string subject, string message, EventStatusModel eventStatusModel = null, User replyTo = null);
}
