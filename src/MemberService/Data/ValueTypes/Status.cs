namespace MemberService.Data.ValueTypes;

using System.ComponentModel.DataAnnotations;

public enum Status
{
    Unknown = 0,

    [Display(Name = "Påmeldt", Description = "Påmeldt", ResourceType = typeof(Status))]
    Pending = 1,

    [Display(Name = "Anbefalt plass", Description = "Påmeldt", ResourceType = typeof(Status))]
    Recommended = 2,

    [Display(Name = "På venteliste", Description = "På venteliste", ResourceType = typeof(Status))]
    WaitingList = 3,

    [Display(Name = "Gitt plass", Description = "Fått plass", ResourceType = typeof(Status))]
    Approved = 4,

    [Display(Name = "Godtatt og betalt", Description = "Deltar", ResourceType = typeof(Status))]
    AcceptedAndPayed = 5,

    [Display(Name = "Takket nei til plass", Description = "Takket nei", ResourceType = typeof(Status))]
    RejectedOrNotPayed = 6,

    [Display(Name = "Ikke fått plass", Description = "Ikke fått plass", ResourceType = typeof(Status))]
    Denied = 7
}
