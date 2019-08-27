using MemberService.Auth.Email;
using MemberService.Data;
using MemberService.Emails.Account;
using MemberService.Emails.Event;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IPartialRenderer _partialRenderer;
        private readonly IFastEmailSender _fastEmailSender;
        private readonly ISlowEmailSender _slowEmailSender;

        public EmailService(
            IPartialRenderer partialRenderer,
            IFastEmailSender fastEmailSender,
            ISlowEmailSender slowEmailSender)
        {
            _partialRenderer = partialRenderer;
            _fastEmailSender = fastEmailSender;
            _slowEmailSender = slowEmailSender;
        }

        public async Task SendLoginEmail(string email, LoginModel model)
        {
            var subject = $"Logg inn - {model.Code} - Bårdar Swing Club";

            var body = await _partialRenderer.RenderPartial("~/Emails/Account/Login.cshtml", model);

            await _fastEmailSender.SendEmailAsync(
                email,
                subject,
                body);
        }

        public async Task<bool> SendEventStatusEmail(string email, Status status, EventStatusModel model)
        {
            var subject = GetEventStatusSubject(status, model.Title);

            if (subject == null)
                return false;

            var body = await _partialRenderer.RenderPartial(GetEventStatusPartial(status, model), model);

            await _slowEmailSender.SendEmailAsync(
                email,
                subject,
                body);

            return true;
        }

        public async Task SendCustomEmail(string email, string subject, string message, EventStatusModel eventStatusModel)
        {
            await _slowEmailSender.SendEmailAsync(
                email,
                Replace(subject, eventStatusModel),
                Markdig.Markdown.ToHtml(Replace(message, eventStatusModel)));
        }

        private static string GetEventStatusSubject(Status status, string title)
        {
            switch (status)
            {
                case Status.Approved:
                    return $"Du har fått plass på {title}";
                case Status.WaitingList:
                    return $"Du er på ventelisten til {title}";
                case Status.Denied:
                    return $"Du har mistet plassen din til {title}";
                default:
                    return null;
            }
        }

        private static string GetEventStatusPartial(Status status, EventStatusModel model)
        {
            switch (status)
            {
                case Status.Approved:
                    return "~/Emails/Event/Approved.cshtml";
                case Status.WaitingList:
                    return "~/Emails/Event/WaitingList.cshtml";
                case Status.Denied:
                    return "~/Emails/Event/Denied.cshtml";
                default:
                    return null;
            }
        }

        private static string Replace(string value, EventStatusModel model)
            => value
                .Replace("{TITLE}", model.Title)
                .Replace("{NAME}", model.Name)
                .Replace("{LINK}", model.Link);

    }
}
