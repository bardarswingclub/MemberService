using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;

namespace MemberService.Pages.Semester
{
    public class SemesterModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public IReadOnlyList<Data.Event> Courses { get; set; }

        [Expressionify]
        public static SemesterModel Create(Data.Semester s) => 
            new SemesterModel
            {
                Id = s.Id,
                Title = s.Title,
                Courses = s.Courses
                    .Where(c => c.Archived == false)
                    .OrderBy(c => c.Title)
                    .ToList()
            };
    }
}