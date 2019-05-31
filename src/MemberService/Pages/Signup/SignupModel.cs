using System;
using Clave.Expressionify;
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

        public EventSignup UserEventSignup { get; set; }

        [Expressionify]
        public static SignupModel Create(Data.Event e)
            => new SignupModel
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Options = e.SignupOptions
            };
    }
}