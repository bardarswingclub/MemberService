namespace MemberService.Pages.Signup;

public record AcceptModel
{
    public Guid Id { get; init; }

    public string Title { get; init; }

    public string Description { get; init; }

    public SignupRequirement Requirement { get; init; }

    public decimal MustPayAmount { get; init; }
    public decimal MembersPrice { get; init; }
}
