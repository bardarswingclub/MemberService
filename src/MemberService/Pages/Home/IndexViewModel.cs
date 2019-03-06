using MemberService.Data;
using MemberService.Services;

namespace MemberService.Pages.Home
{
    public class IndexViewModel
    {
        public string Email { get; set; }

        public Fee MembershipFee { get; set; }

        public Fee TrainingFee { get; set; }

        public Fee ClassesFee { get; set; }
    }
}
