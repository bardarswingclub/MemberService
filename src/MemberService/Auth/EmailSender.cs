using System;
using System.Net.Mail;
using System.Threading.Tasks;
using MemberService.Configs;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MemberService.Auth
{
    public class EmailSender : IEmailSender, IDisposable
    {
        private readonly EmailConfig _config;
        private SmtpClient _client;

        public EmailSender(Config config)
        {
            _config = config.Email;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            await SmtpClient.SendMailAsync(new MailMessage
            {
                Sender = new MailAddress(_config.From, "Bårdar Swing Club"),
                From = new MailAddress(_config.From, "Bårdar Swing Club"),
                To = {
                    new MailAddress(email)
                },
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            });
        }

        public void Dispose() => _client?.Dispose();

        private SmtpClient SmtpClient => _client ?? (_client = CreateClient(_config));

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