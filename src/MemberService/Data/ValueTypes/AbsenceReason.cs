namespace MemberService.Data.ValueTypes;

using System.ComponentModel.DataAnnotations;

public enum AbsenceReason
{
    [Display(Name = "Sykdom")]
    Sick = 1,

    [Display(Name = "Ikke møtt")]
    NoShow = 2,

    [Display(Name = "Sluttet kurs")]
    DroppedOut = 3,

    [Display(Name = "Annet")]
    Other = 4
}
