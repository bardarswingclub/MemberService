namespace MemberService.Data.ValueTypes;

using System.ComponentModel.DataAnnotations;

public enum EventType
{
    Unknown,

    [Display(Name = "Kurs", Description = "Kurs (et semester)")]
    Class,

    [Display(Name = "Workshop", Description = "Workshop (en helg)")]
    Workshop,

    [Display(Name = "Fest", Description = "Fest (en kveld)")]
    Party,

    [Display(Name = "Egentrening", Description = "Egentrening (en time)")]
    Training,
}
