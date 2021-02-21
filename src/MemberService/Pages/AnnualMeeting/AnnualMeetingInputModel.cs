using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MemberService.Pages.AnnualMeeting
{
    public class AnnualMeetingInputModel
    {
        public Guid? Id { get; set; }

        [Required]
        [DisplayName("Navn")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Invitasjonstekst")]
        public string Invitation { get; set; }

        [Required]
        [DisplayName("Møteinfo")]
        public string Info { get; set; }

        [Required]
        [DisplayName("Referat")]
        public string Summary { get; set; }

        [Required]
        [DisplayName("Møtet starter")]
        public string MeetingStartsAtDate { get; set; }

        [Required]
        [RegularExpression(@"^\d\d:\d\d$")]
        public string MeetingStartsAtTime { get; set; }

        [Required]
        [DisplayName("Møtet slutter")]
        public string MeetingEndsAtDate { get; set; }

        [Required]
        [RegularExpression(@"^\d\d:\d\d$")]
        public string MeetingEndsAtTime { get; set; }
    }
}