namespace MemberService.Pages.Home;



using Clave.Expressionify;

using MemberService.Data;
using MemberService.Data.ValueTypes;

public partial class CourseSignupModel
{
    public int Priority { get; set; }

    public string Title { get; set; }

    public string UserId { get; set; }

    public Guid EventId { get; set; }

    public Status Status { get; set; }

    public DanceRole Role { get; set; }

    public string PartnerEmail { get; set; }

    public bool RoleSignup { get; set; }

    public bool AllowPartnerSignup { get; set; }

    [Expressionify]
    public static CourseSignupModel Create(EventSignup s) => new()
    {
        Priority = s.Priority,
        Title = s.Event.Title,
        UserId = s.UserId,
        EventId = s.EventId,
        Status = s.Status,
        Role = s.Role,
        PartnerEmail = s.PartnerEmail,
        RoleSignup = s.Event.SignupOptions.RoleSignup,
        AllowPartnerSignup = s.Event.SignupOptions.AllowPartnerSignup,
    };
}
