using System;
using System.Collections.Generic;
using System.Linq;
using Clave.ExtensionMethods;

using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Event
{
    public class EventModel
    {
        private static readonly Status[] Statuses = {
            Status.AcceptedAndPayed,
            Status.Approved,
            Status.WaitingList,
            Status.Recommended,
            Status.Pending,
            Status.RejectedOrNotPayed,
            Status.Denied,
        };

        public Guid Id { get; set; }

        public Guid? SemesterId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyCollection<EventSignupStatusModel> Signups { get; set; }

        public bool Archived { get; set; }

        public EventType Type { get; set; }

        public bool RoleSignup { get; set; }

        public bool AllowPartnerSignup { get; set; }

        public EventFilterModel Filter { get; set; }

        public static EventModel Create(Data.Event model, IReadOnlyList<EventSignupModel> signups)
        {
            return new()
            {
                Id = model.Id,
                SemesterId = model.SemesterId,
                Title = model.Title,
                Description = model.Description,
                Signups = Statuses
                    .Select(s => (s, signups.Where(x => x.Status == s).ToList()))
                    .Select(EventSignupStatusModel.Create)
                    .ToReadOnlyCollection(),
                Archived = model.Archived,
                Type = model.Type,
                RoleSignup = model.SignupOptions.RoleSignup,
                AllowPartnerSignup = model.SignupOptions.AllowPartnerSignup
            };
        }
    }
}
