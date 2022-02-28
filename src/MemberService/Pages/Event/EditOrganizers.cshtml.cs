namespace MemberService.Pages.Event;

using Clave.Expressionify;

using MemberService.Data;

public partial class EditOrganizersModel : EventBaseModel
{
    public IReadOnlyList<Organizer> Organizers { get; set; }

    [Expressionify]
    public static EditOrganizersModel Create(Event e) => new()
    {
        EventDescription = e.Description,
        EventId = e.Id,
        EventTitle = e.Title,
        IsArchived = e.Archived,
        IsCancelled = e.Cancelled,
        SemesterId = e.SemesterId,
        Organizers = e.Organizers.Select(o => new Organizer
        {
            Id = o.UserId,
            FullName = o.User.FullName,
            Email = o.User.Email,
            CanEdit = o.CanEdit,
            CanAddPresenceLesson = o.CanAddPresenceLesson,
            CanEditOrganizers = o.CanEditOrganizers,
            CanEditSignup = o.CanEditSignup,
            CanSetPresence = o.CanSetPresence,
            CanSetSignupStatus = o.CanSetSignupStatus
        }).ToList(),
    };

    public class Organizer
    {
        public string Id { get; set; }

        public object FullName { get; set; }

        public object Email { get; set; }

        public bool CanEdit { get; set; }

        public bool CanEditSignup { get; set; }

        public bool CanSetSignupStatus { get; set; }

        public bool CanEditOrganizers { get; set; }

        public bool CanSetPresence { get; set; }

        public bool CanAddPresenceLesson { get; set; }
    }
}
