using MemberService.Services;

namespace MemberService.Pages.Home
{
    public class IndexViewModel
    {
        public string Email { get; set; }

        public (FeeStatus Status, Fee Fee) MembershipFee { get; set; }

        public (FeeStatus Status, Fee Fee) TrainingFee { get; set; }

        public (FeeStatus Status, Fee Fee) ClassesFee { get; set; }
    }
}
