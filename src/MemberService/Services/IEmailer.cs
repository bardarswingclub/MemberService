using System;
using System.Threading.Tasks;
using SendGrid.Helpers.Mail;

namespace MemberService.Services
{
    public interface IEmailer
    {
        Task Send(EmailAddress to, string subject, string body, EmailAddress from=null);
    }

    public class DummyConsoleEmailer : IEmailer
    {
        public async Task Send(EmailAddress to, string subject, string body, EmailAddress from = null)
        {
#if DEBUG
            await Console.Out.WriteLineAsync($"Email address: {to}");
            await Console.Out.WriteLineAsync($"From: {from}");
            await Console.Out.WriteLineAsync($"Subject: {subject}");
            await Console.Out.WriteLineAsync("Email body:");
            await Console.Out.WriteLineAsync(body);
#else
            throw new System.Exception("Dummy methods are only available in debug builds!");
#endif
        }
    }
}