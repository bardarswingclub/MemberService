using System.ComponentModel.DataAnnotations;

namespace MemberService.Data
{
    public enum EventType
    {
        Unknown,

        [Display(Name = "Kurs", Description = "Kurs (et semester)")]
        Class,

        [Display(Name = "Workshop", Description = "Workshop (en helg)")]
        Workshop,

        [Display(Name = "Fest", Description = "Fest (en kveld)")]
        Party,
    }
}