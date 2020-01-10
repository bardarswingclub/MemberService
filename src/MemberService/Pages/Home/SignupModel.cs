using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;

namespace MemberService.Pages.Home
{
    public class SignupModel
    {
        public IReadOnlyCollection<CourseModel> Courses { get; set; }

        public IReadOnlyCollection<CourseModel> OpenClasses { get; set; }

        public DateTime? OpensAt { get; set; }

        public bool Sortable { get; set; }

        [Expressionify]
        public static SignupModel Create(Data.Semester s, string userId) => new SignupModel
        {
            OpensAt = s.SignupOpensAt,
            OpenClasses = s.Courses
                .Select(c => CourseModel.Create(c, userId))
                .ToList()
        };
    }
}
