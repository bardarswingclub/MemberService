using MemberService.Configs;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace MemberService.Auth.Email
{
    public class SlowEmailSender : ISlowEmailSender
    {
        private readonly EmailConfig _config;
        private readonly SendGridClient _client;

        public SlowEmailSender(EmailConfig config)
        {
            _config = config;
            _client = new SendGridClient(_config.SendGridApiKey);
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = MailHelper.CreateSingleEmail(
                new EmailAddress(_config.From),
                new EmailAddress(email),
                subject,
                "Epostleseren din er ikke støttet, prøv en annen!",
                htmlMessage);

            await _client.SendEmailAsync(message);
        }
    }
}