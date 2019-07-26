using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MemberService.Data
{
    public enum EventType
    {
        Unknown,

        [DisplayName("Kurs")]
        [Display(Description = "Kurs (et semester)")]
        Class,

        [DisplayName("Workshop")]
        [Display(Description = "Workshop (en helg)")]
        Workshop,

        [DisplayName("Fest")]
        [Display(Description = "Fest (en kveld)")]
        Party,
    }
}