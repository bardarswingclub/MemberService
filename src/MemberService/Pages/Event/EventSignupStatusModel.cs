using Clave.ExtensionMethods;
using MemberService.Data;
using MemberService.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MemberService.Pages.Event
{
    public class EventSignupStatusModel
    {
        public Status Status { get; set; }

        public IReadOnlyCollection<EventSignupModel> Signups { get; set; }

        public string Key => Status.ToString();

        public string Display => Status.GetAttribute<DisplayNameAttribute>().DisplayName;

        public bool Active => Status == Status.Pending;

        public IReadOnlyCollection<EventSignupModel> Leads => Signups.Where(s => s.Role == DanceRole.Lead).ToReadOnlyCollection();

        public IReadOnlyCollection<EventSignupModel> Follows => Signups.Where(s => s.Role == DanceRole.Follow).ToReadOnlyCollection();

        public IReadOnlyCollection<EventSignupModel> Solos => Signups.Where(s => s.Role == DanceRole.None).ToReadOnlyCollection();

        public static EventSignupStatusModel Create(Status status, IEnumerable<EventSignup> signups)
        {
            return new EventSignupStatusModel
            {
                Status = status,
                Signups = signups.Select(EventSignupModel.Create).ToReadOnlyCollection()
            };
        }
    }
}