using System;
using System.Collections.Generic;
using System.Linq;
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

        public User User { get; set; }

        public EventSignup UserEventSignup { get; set; }

        public IReadOnlyList<SignupQuestion> Questions { get; set; }

        public bool HasClosed { get; set; }

        public bool IsOpen { get; set; }

        [Expressionify]
        public static SignupModel Create(Data.Event e)
            => new SignupModel
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Options = e.SignupOptions,
                IsOpen = e.IsOpen(),
                HasClosed = e.HasClosed(),
                Questions = e.Questions.Select(q => SignupQuestion.Create(q)).ToList()
            };
    }
}