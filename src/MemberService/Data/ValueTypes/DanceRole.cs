using System.ComponentModel;

namespace MemberService.Data.ValueTypes
{
    public enum DanceRole
    {
        [DisplayName("Solo")]
        None,

        [DisplayName("Fører")]
        Lead,

        [DisplayName("Følger")]
        Follow
    }
}