using System.ComponentModel;

namespace MemberService.Data
{
    public enum DanceRole
    {
        None,

        [DisplayName("Fører")]
        Lead,

        [DisplayName("Følger")]
        Follow
    }
}