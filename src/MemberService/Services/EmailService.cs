using MemberService.Emails.Account;
using MemberService.Emails.Event;
using System.Threading.Tasks;
using MemberService.Data;

using SendGrid.Helpers.Mail;

namespace MemberService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IPartialRenderer _partialRenderer;
        private readonly IEmailer _emailer;

        public EmailService(
            IPartialRenderer partialRenderer, IEmailer emailer)
        {
            _partialRenderer = partialRenderer;
            _emailer = emailer;
        }

        public async Task SendLoginEmail(string email, string name, LoginModel model)
        {
            var subject = $"Logg inn - {model.Code} - Bårdar Swing Club";

            var body = await _partialRenderer.RenderPartial("~/Emails/Account/Login.cshtml", model);

            await _emailer.Send(
                new EmailAddress(email, name),
                subject,
                body);
        }

        public async Task SendCustomEmail(User to, string subject, string message, EventStatusModel eventStatusModel = null, User replyTo = null)
        {
            await _emailer.Send(
                new EmailAddress(to.Email, to.FullName),
                Replace(subject, to, eventStatusModel),
                Markdig.Markdown.ToHtml(Replace(message, to, eventStatusModel)),
                replyTo != null ? new EmailAddress(replyTo.Email, replyTo.FullName) : null);
        }

        private static string Replace(string value, User user, EventStatusModel model)
            => value
                .Replace("{TITLE}", model?.Title ?? string.Empty)
                .Replace("{NAME}", user.FullName)
                .Replace("{LINK}", model?.Link ?? string.Empty);

    }
}
