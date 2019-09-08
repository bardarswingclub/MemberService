using System;
using System.Collections.Generic;
using System.Linq;
using Clave.ExtensionMethods;

namespace MemberService.Pages.Event.Presence
{
    public class PresenceModel
    {
        public PresenceModel(Data.Event e)
        {
            Id = e.Id;
            Title = e.Title;
            Count = e.LessonCount;
            Roles = e.Signups
                .GroupBy(x => x.Role, x => x, (role, signups) => new PresenceRoleModel(role, signups, e.LessonCount))
                .ToReadOnlyCollection();
        }

        public Guid Id { get; }

        public string Title { get; }

        public int Count { get; }

        public IReadOnlyCollection<PresenceRoleModel> Roles { get; }
    }
}
