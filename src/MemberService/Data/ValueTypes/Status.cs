namespace MemberService.Data.ValueTypes;

using System.ComponentModel.DataAnnotations;

public enum Status
{
    Unknown = 0,

    [Display(Name = "Påmeldt", Description = "Påmeldt")]
    Pending = 1,

    [Display(Name = "Anbefalt plass", Description = "Påmeldt")]
    Recommended = 2,

    [Display(Name = "På venteliste", Description = "På venteliste")]
    WaitingList = 3,

    [Display(Name = "Gitt plass", Description = "Fått plass")]
    Approved = 4,

    [Display(Name = "Godtatt og betalt", Description = "Deltar")]
    AcceptedAndPayed = 5,

    [Display(Name = "Takket nei til plass", Description = "Takket nei")]
    RejectedOrNotPayed = 6,

    [Display(Name = "Ikke fått plass", Description = "Ikke fått plass")]
    Denied = 7
}
