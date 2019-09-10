using System.Collections.Generic;
using System.Linq;
using Clave.ExtensionMethods;
using MemberService.Data;

namespace MemberService.Pages.Event.Presence
{
    public class PresenceRoleModel
    {
        public PresenceRoleModel(DanceRole role, IEnumerable<EventSignup> signups, int count)
        {
            Role = role;
            Participants = signups
                .Where(s => s.Status == Status.AcceptedAndPayed || s.Status == Status.Approved)
                .Select(s => new ParticipantsModel(s, count))
                .OrderBy(p => p.FullName)
                .ToReadOnlyCollection();
        }

        public DanceRole Role { get; }

        public IReadOnlyCollection<ParticipantsModel> Participants { get; }
    }
}