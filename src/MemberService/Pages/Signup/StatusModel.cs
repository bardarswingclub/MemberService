namespace MemberService.Pages.Signup;

using MemberService.Data.ValueTypes;

public class StatusModel
{
    public Guid Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public bool IsCancelled { get; init; }
    public bool IsArchived { get; init; }
    public Status Status { get; init; }
    public Guid? SurveyId { get; init; }
    public bool? Refunded { get; init; }
    public bool RoleSignup { get; init; }
    public bool AllowPartnerSignup { get; init; }
    public DanceRole Role { get; init; }
    public string PartnerEmail { get; init; }
}
