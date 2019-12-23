using System.ComponentModel.DataAnnotations;

namespace MemberService.Data.ValueTypes
{
    public enum DanceRole
    {
        [Display(Name = "Solo")]
        None,

        [Display(Name = "Fører")]
        Lead,

        [Display(Name = "Følger")]
        Follow
    }
}