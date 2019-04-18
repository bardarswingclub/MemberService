using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MemberService.Pages.Members
{
    public class ManualPaymentModel
    {
        [Required]
        [DisplayName("Beløp")]
        public int Amount { get; set; }

        [DisplayName("Inkluderer medlemskap")]
        public bool IncludesMembership { get; set; }

        [DisplayName("Inkluderer treningsavgift")]
        public bool IncludesTraining { get; set; }

        [DisplayName("Inkluderer kursavgift")]
        public bool IncludesClasses { get; set; }

        [Required]
        [DisplayName("Beskrivelse")]
        public string Description { get; set; }
    }
}