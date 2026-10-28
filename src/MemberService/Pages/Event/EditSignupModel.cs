namespace MemberService.Pages.Event;





using Clave.Expressionify;

using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.EntityFrameworkCore;

public partial class EditSignupModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public DanceRole Role { get; set; }

    public RolePreference RolePreferences { get; set; }

    public string PartnerEmail { get; set; }

    public User Partner { get; set; }

    public Guid EventId { get; set; }

    public string FullName { get; set; }

    public RolePreference SelectedPreference => RolePreferences != RolePreference.None ? RolePreferences : RolePreferenceExtensions.FromRole(Role);

    public EventType Type { get; set; }

    public string Origin { get; set; }

    public string SpecialNeeds { get; set; }

    public IReadOnlyList<Data.Event> Courses { get; set; }

    [Expressionify]
    public static EditSignupModel Create(EventSignup e, DbSet<User> users) => new()
    {
        Id = e.Id,
        Title = e.Event.Title,
        FullName = e.User.FullName,
        EventId = e.EventId,
        Type = e.Event.Type,
        Origin = e.Origin,
        SpecialNeeds = e.SpecialNeeds,
        Role = e.Role,
        RolePreferences = e.RolePreferences,
        PartnerEmail = e.PartnerEmail,
        Partner = users.FirstOrDefault(u => e.PartnerEmail == u.NormalizedEmail),
        Courses = e.Event.Semester.Courses.ToList()
    };
}
