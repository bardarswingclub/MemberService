using System.Net.Mail;
using System.Threading.Tasks;
using MemberService.Configs;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MemberService.Auth
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfig _config;

        public EmailSender(Config config)
        {
            _config = config.Email;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mail = new MailMessage
            {
                Sender = new MailAddress(_config.From, "Bårdar Swing Club"),
                From = new MailAddress(_config.From, "Bårdar Swing Club"),
                To = {
                    new MailAddress(email)
                },
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            await CreateClient(_config).SendMailAsync(mail);
        }

        private static SmtpClient CreateClient(EmailConfig config)
            => new SmtpClient(config.Host, config.Port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(config.Username, config.Password),
                EnableSsl = true
            };
    }
}