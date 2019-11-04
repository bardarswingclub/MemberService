using MemberService.Emails.Account;
using MemberService.Emails.Event;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MemberService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IPartialRenderer _partialRenderer;
        private readonly IEmailSender _emailSender;

        public EmailService(
            IPartialRenderer partialRenderer, IEmailSender emailSender)
        {
            _partialRenderer = partialRenderer;
            _emailSender = emailSender;
        }

        public async Task SendLoginEmail(string email, LoginModel model)
        {
            var subject = $"Logg inn - {model.Code} - Bårdar Swing Club";

            var body = await _partialRenderer.RenderPartial("~/Emails/Account/Login.cshtml", model);

            await _emailSender.SendEmailAsync(
                email,
                subject,
                body);
        }

        public async Task SendCustomEmail(string email, string subject, string message, EventStatusModel eventStatusModel)
        {
            await _emailSender.SendEmailAsync(
                email,
                Replace(subject, eventStatusModel),
                Markdig.Markdown.ToHtml(Replace(message, eventStatusModel)));
        }

        private static string Replace(string value, EventStatusModel model)
            => value
                .Replace("{TITLE}", model.Title)
                .Replace("{NAME}", model.Name)
                .Replace("{LINK}", model.Link);

    }
}
