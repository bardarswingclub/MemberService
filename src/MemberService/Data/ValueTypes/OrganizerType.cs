namespace MemberService.Data.ValueTypes
{
    using System.ComponentModel.DataAnnotations;

    public enum OrganizerType
    {
        Unknown,

        [Display(Name = "Eier")]
        Owner,

        [Display(Name = "Koordinator")]
        Coordinator,

        [Display(Name = "Instruktør")]
        Instructor
    }
}