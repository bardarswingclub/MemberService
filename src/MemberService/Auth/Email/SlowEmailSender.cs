using System;
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

        public SlowEmailSender(SendGridClient client, Config config)
        {
            _client = client;
            _config = config.Email;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = MailHelper.CreateSingleEmail(
                new EmailAddress(_config.From),
                new EmailAddress(email),
                subject,
                "Epostleseren din er ikke støttet, prøv en annen!",
                htmlMessage);

            var response =  await _client.SendEmailAsync(message);
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