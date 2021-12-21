namespace MemberService.Pages.Home;

using MemberService.Services;

public class FeesViewModel
{
    public (FeeStatus Status, Fee Fee) MembershipFee { get; set; }

    public (FeeStatus Status, Fee Fee) TrainingFee { get; set; }

    public (FeeStatus Status, Fee Fee) ClassesFee { get; set; }
}
