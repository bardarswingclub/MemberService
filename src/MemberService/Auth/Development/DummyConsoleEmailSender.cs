using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MemberService.Auth.Development
{
    public class DummyConsoleEmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
#if DEBUG
            await Console.Out.WriteLineAsync($"Email address: {email}");
            await Console.Out.WriteLineAsync($"Subject: {subject}");
            await Console.Out.WriteLineAsync("Email body:");
            await Console.Out.WriteLineAsync(htmlMessage);
#else
            throw new System.Exception("Dummy methods are only available in debug builds!");
#endif
        }
    }
}
