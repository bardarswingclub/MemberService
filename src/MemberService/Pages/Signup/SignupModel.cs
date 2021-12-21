namespace MemberService.Pages.Signup;





using Clave.Expressionify;

using MemberService.Data;

public partial class SignupModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public EventSignupOptions Options { get; set; }

    public SignupInputModel Input { get; set; }

    public User User { get; set; }

    public EventSignup UserEventSignup { get; set; }

    public IReadOnlyList<SignupQuestion> Questions { get; set; }

    public bool HasClosed { get; set; }

    public bool IsArchived { get; set; }

    public bool IsCancelled { get; set; }

    public bool IsOpen { get; set; }

    [Expressionify]
    public static SignupModel Create(Data.Event e)
        => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Options = e.SignupOptions,
            IsOpen = e.IsOpen(),
            HasClosed = e.HasClosed(),
            Questions = e.Survey.Questions.Select(q => SignupQuestion.Create(q)).ToList(),
            IsArchived = e.Archived,
            IsCancelled = e.Cancelled
        };
}
