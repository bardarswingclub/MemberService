namespace MemberService.Pages.Event;




public class ParticipantsModel
{
    public ParticipantsModel(IReadOnlyList<EventSignupModel> signups, EventModel @event)
    {
        Signups = signups;
        AllowPartnerSignup = @event.AllowPartnerSignup;
        ShowPriority = @event.SemesterId is Guid;
        EventId = @event.Id;
    }

    public IReadOnlyList<EventSignupModel> Signups { get; }

    public EventSignupModel this[int index] => Signups[index];

    public bool AllowPartnerSignup { get; }

    public bool ShowPriority { get; }

    public Guid EventId { get; }
}
