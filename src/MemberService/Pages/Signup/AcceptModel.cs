namespace MemberService.Pages.Signup;

public record AcceptModel
{
    public enum AcceptanceRequirement
    {
        None,
        MustPayClassesFee,
        MustPayTrainingFee,
        MustPayMembershipFee,
        MustPayMembersPrice,
        MustPayNonMembersPrice
    }

    public Guid Id { get; init; }

    public string Title { get; init; }

    public string Description { get; init; }

    public AcceptanceRequirement Requirement { get; init; }

    public decimal MustPayAmount { get; init; }
}
