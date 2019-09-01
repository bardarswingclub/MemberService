using System.ComponentModel;

namespace MemberService.Data
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