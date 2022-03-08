namespace MemberService.Pages.Event;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public abstract class EventInputModel : PageModel
{
    [BindProperty]
    public Guid? SemesterId { get; set; }

    [BindProperty]
    [DisplayName("Type")]
    public EventType Type { get; set; }

    [BindProperty]
    [DisplayName("Navn")]
    public string Title { get; set; }

    [BindProperty]
    [DisplayName("Beskrivelse")]
    public string Description { get; set; }

    [BindProperty]
    public bool EnableSignupOpensAt { get; set; }

    [BindProperty]
    [DisplayName("Påmeldingen åpner")]
    public string SignupOpensAtDate { get; set; }

    [BindProperty]
    [RegularExpression(@"^\d\d:\d\d$")]
    public string SignupOpensAtTime { get; set; }

    [BindProperty]
    public bool EnableSignupClosesAt { get; set; }

    [BindProperty]
    [DisplayName("Påmelding stenger")]
    public string SignupClosesAtDate { get; set; }

    [BindProperty]
    [RegularExpression(@"^\d\d:\d\d$")]
    public string SignupClosesAtTime { get; set; }

    [BindProperty]
    [DisplayName("Krever medlemskap")]
    public bool RequiresMembershipFee { get; set; }

    [BindProperty]
    [DisplayName("Krever betalt treningsavgift")]
    public bool RequiresTrainingFee { get; set; }

    [BindProperty]
    [DisplayName("Krever betalt kursavgift")]
    public bool RequiresClassesFee { get; set; }

    [BindProperty]
    [DisplayName("Pris for medlemmer")]
    public decimal PriceForMembers { get; set; }

    [BindProperty]
    [DisplayName("Pris for ikke-medlemmer")]
    public decimal PriceForNonMembers { get; set; }

    [BindProperty]
    [DisplayName("Gratis hvis man har betalt treningsavgift?")]
    public bool IncludedInTrainingFee { get; set; }

    [BindProperty]
    [DisplayName("Gratis hvis man har betalt kursavgift?")]
    public bool IncludedInClassesFee { get; set; }

    [BindProperty]
    [DisplayName("Hjelpetekst")]
    public string SignupHelp { get; set; }

    [BindProperty]
    [DisplayName("Fører og følger")]
    public bool RoleSignup { get; set; }

    [BindProperty]
    [DisplayName("Hjelpetekst")]
    public string RoleSignupHelp { get; set; }

    [BindProperty]
    [DisplayName("La par melde seg på sammen")]
    public bool AllowPartnerSignup { get; set; }

    [BindProperty]
    [DisplayName("Hjelpetekst")]
    public string AllowPartnerSignupHelp { get; set; }

    [BindProperty]
    [DisplayName("Automatisk tildelte plasser")]
    public int AutoAcceptedSignups { get; set; }
}
