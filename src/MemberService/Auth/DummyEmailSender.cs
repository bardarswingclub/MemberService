﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MemberService.Auth
{
    public class DummyEmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await Console.Out.WriteLineAsync($"Email address: {email}");
            await Console.Out.WriteLineAsync($"Subject: {subject}");
            await Console.Out.WriteLineAsync($"Email body:");
            await Console.Out.WriteLineAsync(htmlMessage);
        }
    }
}
