using MemberService.Data;
using MemberService.Services;

namespace MemberService.Pages.Home
{
    public class IndexViewModel
    {
        public MemberUser User { get; set; }

        public Fee MembershipFee { get; set; }
    }
}
