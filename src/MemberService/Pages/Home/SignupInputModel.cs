namespace MemberService.Pages.Home;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;  // for [Required]
using Microsoft.AspNetCore.Mvc.Rendering;      // for SelectListItem
using MemberService.Data.ValueTypes;          // for SomeConsentState
using MemberService.Data;          // for SomeConsentState
using MemberService.Pages.Shared;             // your EnforceTrue attribute

public class ConsentInputModel
{
    [Required(ErrorMessage = "Du må ta et valg om samtykke.")]
    [DisplayName("Samtykke for sosiale medier")]
    public SomeConsentState? SelectedState { get; set; }

    // Dropdown options
    public List<SelectListItem> Options { get; set; } = new();
}


public class SignupInputModel
{
    public DateTime SignupOpensAt { get; set; }

    [DisplayName("Jeg godtar rettningslinjene til Bårdar Swing Club")]
    [EnforceTrue(ErrorMessage = "Du må godta våre rettningslinjer for å delta på kurs")]
    public bool Accept { get; set; }

    public ConsentInputModel NewConsent { get; set; } = new();

    public string SignupHelpText { get; set; }
    
}
