using System;
using System.ComponentModel;
using MemberService.Pages.Shared;

namespace MemberService.Pages.Home
{
    public class SignupInputModel
    {
        public DateTime SignupOpensAt { get; set; }

        [DisplayName("Jeg godtar rettningslinjene til Bårdar Swing Club")]
        [EnforceTrue(ErrorMessage = "Du må godta våre rettningslinjer for å delta på kurs")]
        public bool Accept { get; set; }

        public string SignupHelpText { get; set; }
    }
}