using System;
using MemberService.Data;

namespace MemberService.Pages.Signup
{
    public class SignupModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public EventSignupOptions Options { get; set; }

        public SignupInputModel Input { get; set; }

        public MemberUser User { get; set; }
    }
}