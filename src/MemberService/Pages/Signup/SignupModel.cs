namespace MemberService.Pages.Signup;
public class SignupModel
{
    public string Title { get; init; }

    public string Description { get; init; }

    public decimal PriceForMembers { get; init; }

    public decimal PriceForNonMembers { get; init; }

    public string SignupHelp { get; init; }

    public bool RoleSignup { get; init; }

    public string RoleSignupHelp { get; init; }

    public bool AllowPartnerSignup { get; init; }

    public string AllowPartnerSignupHelp { get; init; }

    public SignupInputModel Input { get; init; }

    public Guid? SurveyId { get; init; }

    public SignupRequirement Requirement { get; init; }
}
