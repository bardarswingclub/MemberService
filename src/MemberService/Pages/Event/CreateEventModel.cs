using System;
using System.ComponentModel.DataAnnotations;

namespace MemberService.Pages.Event
{
    public class CreateEventModel
    {
        [Display(Name = "Navn")]
        public string Title { get; set; }

        [Display(Name = "Beskrivelse")]
        public string Description { get; set; }

        [Display(Name = "Påmeldingen åpner")]
        public DateTime? SignupOpensAt { get; set; }

        [Display(Name = "Påmelding stenger")]
        public DateTime? SignupClosesAt { get; set; }

        [Display(Name = "Krever medlemskap")]
        public bool RequiresMembershipFee { get; set; }

        [Display(Name = "Krever betalt treningsavgift")]
        public bool RequiresTrainingFee { get; set; }

        [Display(Name = "Krever betalt kursavgift")]
        public bool RequiresClassesFee { get; set; }

        [Display(Name = "Pris for medlemmer")]
        public decimal PriceForMembers { get; set; }

        [Display(Name = "Pris for ikke-medlemmer")]
        public decimal PriceForNonMembers { get; set; }

        [Display(Name = "La par melde seg på sammen")]
        public bool AllowPartnerSignup { get; set; }

        [Display(Name = "Fører og følger")]
        public bool RoleSignup { get; set; }
    }
}