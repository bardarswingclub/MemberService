namespace MemberService.Services;

using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

using MemberService.Emails.Account;
using MemberService.Emails.Event;

using MemberService.Data;

using Markdig;

public class EmailService : IEmailService
{
    private readonly IPartialRenderer _partialRenderer;
//    private readonly IEmailer _emailer;
    private readonly IConfiguration _configuration;

    public EmailService(
        IPartialRenderer partialRenderer, IConfiguration configuration)
    {
        _partialRenderer = partialRenderer;
        _configuration = configuration;
    }

    public async Task SendLoginEmail(string email, string name, LoginModel model)
    {
        var subject = $"Logg inn - {model.Code} - Bårdar Swing Club";

        var body = await _partialRenderer.RenderPartial("~/Emails/Account/Login.cshtml", model);

        await SendEmailAsync(
            email, name,
            subject,
            body);
    }

    public async Task SendCustomEmail(User to, string subject, string message, EventStatusModel eventStatusModel = null, User replyTo = null)
    {
        await SendEmailAsync(
            to.Email, to.FullName,
            Replace(subject, to, eventStatusModel),
            Markdown.ToHtml(Replace(message, to, eventStatusModel)),
            replyTo != null ? new MailboxAddress(replyTo.FullName, replyTo.Email) : null);
    }
    
    public async Task SendEmailAsync(string toAddress, string toName, string subject, string message, MailboxAddress replyTo = null)
    {
#if DEBUG
        await Console.Out.WriteLineAsync($"Email address: {toAddress} ({toName})");
        await Console.Out.WriteLineAsync($"From: {replyTo}");
        await Console.Out.WriteLineAsync($"Subject: {subject}");
        await Console.Out.WriteLineAsync("Email body:");
        await Console.Out.WriteLineAsync(message);
#else
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(_configuration["SmtpSettings:SenderName"], _configuration["SmtpSettings:SenderEmail"]));
        emailMessage.To.Add(new MailboxAddress(toName, toAddress));
        if (replyTo != null){
            emailMessage.ReplyTo.Add(replyTo);
        }
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = message };
        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_configuration["SmtpSettings:Server"], int.Parse(_configuration["SmtpSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_configuration["SmtpSettings:Username"], _configuration["SmtpSettings:Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
#endif
    }

    private static string Replace(string value, User user, EventStatusModel model)
        => value
            .Replace("{TITLE}", model?.Title ?? string.Empty)
            .Replace("{NAME}", user.GetFriendlyName())
            .Replace("{LINK}", model?.Link ?? string.Empty);

}
