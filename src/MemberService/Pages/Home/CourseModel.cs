using System;
using System.Linq;
using Clave.Expressionify;
using MemberService.Data;

namespace MemberService.Pages.Home
{
    public partial class CourseModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string SignupHelp { get; set; }

        public DateTime? OpensAt { get; set; }

        public bool IsOpen { get; set; }

        public bool RoleSignup { get; set; }

        public string RoleSignupHelp { get; set; }

        public bool AllowPartnerSignup { get; set; }

        public string AllowPartnerSignupHelp { get; set; }

        public CourseSignupModel Signup { get; set; }

        [Expressionify]
        public static CourseModel Create(Data.Event e, string userId) => new CourseModel
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            SignupHelp = e.SignupOptions.SignupHelp,
            OpensAt = e.SignupOptions.SignupOpensAt,
            IsOpen = e.IsOpen(),
            RoleSignup = e.SignupOptions.RoleSignup,
            RoleSignupHelp = e.SignupOptions.RoleSignupHelp,
            AllowPartnerSignup = e.SignupOptions.AllowPartnerSignup,
            AllowPartnerSignupHelp = e.SignupOptions.AllowPartnerSignupHelp,
            Signup = e.Signups
                .Select(s => CourseSignupModel.Create(s))
                .FirstOrDefault(s => s.UserId == userId)
        };
    }
}