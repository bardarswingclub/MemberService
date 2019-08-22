using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MemberService.Data;

namespace MemberService.Pages.Program
{
    public class ProgramInputModel
    {
        public int Id { get; set; }

        [DisplayName("Navn")]
        [Required]
        public string Title { get; set; }

        [DisplayName("Beskrivelse")]
        [Required]
        public string Description { get; set; }

        public bool EnableSignupOpensAt { get; set; }

        [DisplayName("Påmeldingen åpner")]
        public string SignupOpensAtDate { get; set; }

        [RegularExpression(@"^\d\d:\d\d$")]
        public string SignupOpensAtTime { get; set; }

        public bool EnableSignupClosesAt { get; set; }

        [DisplayName("Påmelding stenger")]
        public string SignupClosesAtDate { get; set; }

        [RegularExpression(@"^\d\d:\d\d$")]
        public string SignupClosesAtTime { get; set; }

        [Required]
        [DisplayName("Påmeldingstype")]
        public ProgramType Type { get; set; }
    }
}
