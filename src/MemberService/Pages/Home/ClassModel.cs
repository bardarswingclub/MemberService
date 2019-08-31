using System;
using System.Linq;
using Clave.Expressionify;

namespace MemberService.Pages.Home
{
    public class ClassModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string SignupHelp { get; set; }

        public DateTime? OpensAt { get; set; }

        public bool RoleSignup { get; set; }

        public string RoleSignupHelp { get; set; }

        public bool AllowPartnerSignup { get; set; }

        public string AllowPartnerSignupHelp { get; set; }

        public ClassSignupModel Signup { get; set; }

        [Expressionify]
        public static ClassModel Create(Data.Event e, string userId) => new ClassModel
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            SignupHelp = e.SignupOptions.SignupHelp,
            OpensAt = e.SignupOptions.SignupOpensAt,
            RoleSignup = e.SignupOptions.RoleSignup,
            RoleSignupHelp = e.SignupOptions.RoleSignupHelp,
            AllowPartnerSignup = e.SignupOptions.AllowPartnerSignup,
            AllowPartnerSignupHelp = e.SignupOptions.AllowPartnerSignupHelp,
            Signup = e.Signups
                .Select(s => ClassSignupModel.Create(s))
                .FirstOrDefault(s => s.UserId == userId)
        };
    }
}