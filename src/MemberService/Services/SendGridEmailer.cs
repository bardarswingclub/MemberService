namespace MemberService.Services;

using MemberService.Configs;

using SendGrid;
using SendGrid.Helpers.Mail;

public class SendGridEmailer : IEmailer
{
    private readonly Config.EmailConfig _config;
    private readonly SendGridClient _client;

    public SendGridEmailer(SendGridClient client, Config config)
    {
        _client = client;
        _config = config.Email;
    }

    public async Task Send(EmailAddress to, string subject, string body, EmailAddress replyTo = null)
    {
        var message = MailHelper.CreateSingleEmail(
            new EmailAddress(_config.From, "Bårdar Swing Club"),
            to,
            subject,
            "Epostleseren din er ikke støttet, prøv en annen!",
            body);

        message.ReplyTo = replyTo;

        var response = await _client.SendEmailAsync(message);
        await ValidateResponse(response);
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private async Task ValidateResponse(Response response)
    {
        if ((int)response.StatusCode < 200 || (int)response.StatusCode > 299)
        {
            throw new Exception("Utsending av epost feilet!", new Exception(await response.Body.ReadAsStringAsync()));
        }
    }
}
