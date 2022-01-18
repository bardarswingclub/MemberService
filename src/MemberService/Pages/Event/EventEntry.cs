namespace MemberService.Pages.Event;

using Clave.Expressionify;

using MemberService.Data;
using MemberService.Data.ValueTypes;

public partial class EventEntry
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public EventType Type { get; set; }

    public bool Archived { get; set; }

    public DateTime? SignupOpensAt { get; set; }

    public DateTime? SignupClosesAt { get; set; }

    public bool RoleSignup { get; set; }

    public bool CanView { get; set; }

    public IReadOnlyList<EventSignup> Signups { get; set; }

    [Expressionify]
    public static EventEntry Create(Event e, string userId) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Type = e.Type,
        Archived = e.Archived,
        SignupOpensAt = e.SignupOptions.SignupOpensAt,
        SignupClosesAt = e.SignupOptions.SignupClosesAt,
        RoleSignup = e.SignupOptions.RoleSignup,
        CanView = e.Organizers.AsQueryable().Any(o => o.UserId == userId),
        Signups = e.Signups.ToList()
    };

    public string Status()
    {
        if (Archived)
        {
            return "Arkivert";
        }

        if (SignupClosesAt < TimeProvider.UtcNow)
        {
            return $"Stengte {SignupClosesAt.Value.ToOsloDate()}";
        }

        if (SignupOpensAt > TimeProvider.UtcNow)
        {
            return $"Åpner {SignupOpensAt.Value.ToOsloDate()}";
        }

        if (SignupOpensAt < TimeProvider.UtcNow)
        {
            return $"Åpnet {SignupOpensAt.Value.ToOsloDate()}";
        }

        return "Åpen";
    }
}
