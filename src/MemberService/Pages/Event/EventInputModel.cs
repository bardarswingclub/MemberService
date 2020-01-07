using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Event
{
    public class EventInputModel : EventBaseModel
    {
        public Guid Id { get; set; }

        [DisplayName("Type")]
        public EventType Type { get; set; }

        public string Title { get; set; }

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

        [DisplayName("Krever medlemskap")]
        public bool RequiresMembershipFee { get; set; }

        [DisplayName("Krever betalt treningsavgift")]
        public bool RequiresTrainingFee { get; set; }

        [DisplayName("Krever betalt kursavgift")]
        public bool RequiresClassesFee { get; set; }

        [DisplayName("Pris for medlemmer")]
        public decimal PriceForMembers { get; set; }

        [DisplayName("Pris for ikke-medlemmer")]
        public decimal PriceForNonMembers { get; set; }

        [DisplayName("Hjelpetekst")]
        public string SignupHelp { get; set; }

        [DisplayName("Fører og følger")]
        public bool RoleSignup { get; set; }

        [DisplayName("Hjelpetekst")]
        public string RoleSignupHelp { get; set; }

        [DisplayName("La par melde seg på sammen")]
        public bool AllowPartnerSignup { get; set; }

        [DisplayName("Hjelpetekst")]
        public string AllowPartnerSignupHelp { get; set; }

        [DisplayName("Automatisk tildelte plasser")]
        public int AutoAcceptedSignups { get; set; }
    }
}