using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MemberService.Pages.Shared;

namespace MemberService.Pages.Home
{
    public class SignupInputModel
    {
        public DateTime SignupOpensAt { get; set; }

        [DisplayName("Jeg godtar rettningslinjerne til Bårdar Swing Club")]
        [EnforceTrue(ErrorMessage = "Du må godta våre rettningslinjer for å delta på kurs")]
        public bool Accept { get; set; }
    }
}