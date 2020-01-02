using Clave.ExtensionMethods;
using MemberService.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Event
{
    public class EventSignupStatusModel
    {
        public Status Status { get; set; }

        public IReadOnlyList<EventSignupModel> Signups { get; set; }

        public string Key => Status.ToString();

        public string Display => Status.DisplayName();

        public bool Active => Status == Status.Pending;

        public IReadOnlyList<EventSignupModel> Leads => Signups.Where(s => s.Role == DanceRole.Lead).ToReadOnlyList();

        public IReadOnlyList<EventSignupModel> Follows => Signups.Where(s => s.Role == DanceRole.Follow).ToReadOnlyList();

        public IReadOnlyList<EventSignupModel> Solos => Signups.Where(s => s.Role == DanceRole.None).ToReadOnlyList();

        public static EventSignupStatusModel Create(Status status, IEnumerable<EventSignup> signups)
        {
            return new EventSignupStatusModel
            {
                Status = status,
                Signups = signups
                    .Select(EventSignupModel.Create)
                    .ToReadOnlyList()
            };
        }
    }
}