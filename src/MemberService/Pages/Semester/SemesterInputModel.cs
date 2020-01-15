using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MemberService.Pages.Semester
{
    public class SemesterInputModel
    {
        public Guid? Id { get; set; }

        [Required]
        [DisplayName("Navn")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Påmeldingen åpner")]
        public string SignupOpensAtDate { get; set; }

        [Required]
        [RegularExpression(@"^\d\d:\d\d$")]
        public string SignupOpensAtTime { get; set; }
    }
}