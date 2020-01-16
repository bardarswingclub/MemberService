using System;
using System.Threading.Tasks;
using MemberService.Configs;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MemberService.Services
{
    public class SendGridEmailer : IEmailer
    {
        private readonly EmailConfig _config;
        private readonly SendGridClient _client;

        public SendGridEmailer(SendGridClient client, Config config)
        {
            _client = client;
            _config = config.Email;
        }

        public async Task Send(EmailAddress to, string subject, string body, EmailAddress replyTo=null)
        {
            var message = MailHelper.CreateSingleEmail(
                new EmailAddress(_config.From, "Bårdar Swing Club"),
                to,
                subject,
                "Epostleseren din er ikke støttet, prøv en annen!",
                body);

            message.ReplyTo = replyTo;

            var response = await _client.SendEmailAsync(message);
            ValidateResponse(response);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void ValidateResponse(Response response)
        {
            if ((int)response.StatusCode < 200 || (int)response.StatusCode > 299)
            {
                throw new Exception("Utsending av epost feilet!");
            }
        }
    }
}