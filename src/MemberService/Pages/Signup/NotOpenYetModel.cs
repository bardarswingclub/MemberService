namespace MemberService.Pages.Signup;

public class NotOpenYetModel
{
    public string Title { get; init; }
    public DateTime? SignupOpensAt { get; init; }
    public string Description { get; init; }
    public string SignupHelp { get; init; }
    public decimal MembersPrice { get; init; }
    public decimal NonMembersPrice { get; init; }
    public bool RequiresMembership { get; init; }
}
