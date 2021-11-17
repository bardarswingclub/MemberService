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
            SemesterId = e.SemesterId;
            Title = e.Title;
            Description = e.Description;
            Count = e.LessonCount;
            Archived = e.Archived;
            Roles = e.Signups
                .GroupBy(x => x.Role, x => x, (role, signups) => new PresenceRoleModel(role, signups, e.LessonCount))
                .ToReadOnlyCollection();
        }

        public bool Archived { get; }

        public Guid Id { get; }

        public Guid? SemesterId { get; }

        public string Title { get; }

        public string Description { get; }

        public int Count { get; }

        public IReadOnlyCollection<PresenceRoleModel> Roles { get; }
    }
}
