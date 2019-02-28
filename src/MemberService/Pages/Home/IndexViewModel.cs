using MemberService.Data;

namespace MemberService.Pages.Home
{
    public class IndexViewModel
    {
        public bool HasPayedMembershipThisYear { get; set; }

        public MemberUser User { get; internal set; }
    }
}
