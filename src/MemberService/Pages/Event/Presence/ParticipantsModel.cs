namespace MemberService.Pages.Event.Presence;

using MemberService.Data;
using MemberService.Data.ValueTypes;

public class ParticipantsModel
{
    public ParticipantsModel(EventSignup s, int count)
    {
        Id = s.Id;
        UserId = s.UserId;
        FriendlyName = s.User.GetFriendlyName();
        FullName = s.User.FullName;
        Status = s.Status;
        Refunded = s.Payment?.Refunded ?? false;
        Presence = CreateList(s.Presence, count);
        Absences = CreateAbsences(s.Presence, count);
    }

    public Guid Id { get; }

    public string UserId { get; }

    public string FriendlyName { get; }

    public string FullName { get; }

    public Status Status { get; }

    public bool Refunded { get; }

    public IReadOnlyList<bool> Presence { get; }

    public IReadOnlyList<AbsenceEntry> Absences { get; }

    private static IReadOnlyList<bool> CreateList(IEnumerable<Data.Presence> presence, int count)
    {
        var list = new bool[count];
        foreach (var p in presence.Where(x => x.Lesson < count))
        {
            list[p.Lesson] = p.Present;
        }

        return list;
    }

    private static IReadOnlyList<AbsenceEntry> CreateAbsences(IEnumerable<Data.Presence> presence, int count)
    {
        var list = new AbsenceEntry[count];
        foreach (var p in presence.Where(x => x.Lesson < count))
        {
            list[p.Lesson] = new AbsenceEntry(p.AbsenceReason, p.AbsenceNote);
        }

        return list;
    }
}

public record AbsenceEntry(AbsenceReason? Reason, string Note);
