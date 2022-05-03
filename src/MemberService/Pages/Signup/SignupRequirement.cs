namespace MemberService.Pages.Signup;

public enum SignupRequirement
{
    None,
    MustPayClassesFee,
    MustPayClassesFeeAndPrice,
    MustPayTrainingFee,
    MustPayTrainingFeeAndPrice,
    MustBeMember,
    MustBeMemberAndPay,
    MustPayMembersPrice,
    MustPayNonMembersPrice
}
