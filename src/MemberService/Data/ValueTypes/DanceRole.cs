namespace MemberService.Data.ValueTypes;

using System.ComponentModel.DataAnnotations;

public enum DanceRole
{
    [Display(Name = "Solo")]
    None,

    [Display(Name = "Fører")]
    Lead,

    [Display(Name = "Følger")]
    Follow
}
