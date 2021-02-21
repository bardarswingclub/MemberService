using System.Threading.Tasks;

using MemberService.Services;
using Microsoft.AspNetCore.Identity.UI.Services;

using SendGrid.Helpers.Mail;

namespace MemberService.Auth
{
    public class EmailSender : IEmailSender
    {
        private readonly IEmailer _emailer;

        public EmailSender(IEmailer emailer)
        {
            _emailer = emailer;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await _emailer.Send(new EmailAddress(email), subject, htmlMessage);
        }
    }
}