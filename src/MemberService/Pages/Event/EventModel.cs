using System;
using System.Collections.Generic;
using System.Linq;
using Clave.ExtensionMethods;
using MemberService.Data;

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

        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyCollection<EventSignupStatusModel> Signups { get; set; }

        public bool Archived { get; set; }

        public DateTime? SignupOpensAt { get; set; }

        public DateTime? SignupClosesAt { get; set; }

        public bool RoleSignup { get; set; }

        public bool AllowPartnerSignup { get; set; }

        public bool HasClosed { get; set; }

        public bool IsOpen { get; set; }

        public static EventModel Create(Data.Event model)
        {
            foreach (var (partner, signup) in model.Signups
                .Select(s => s.Partner)
                .WhereNotNull()
                .Join(model.Signups, p => p.Id, s => s.UserId))
            {
                partner.EventSignups.Add(signup);
            }

            return new EventModel
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Signups = Statuses
                    .Select(s => (s, model.Signups.Where(x => x.Status == s)))
                    .Select(EventSignupStatusModel.Create)
                    .ToReadOnlyCollection(),
                Archived = model.Archived,
                SignupOpensAt = model.SignupOptions.SignupOpensAt,
                SignupClosesAt = model.SignupOptions.SignupClosesAt,
                RoleSignup = model.SignupOptions.RoleSignup,
                AllowPartnerSignup = model.SignupOptions.AllowPartnerSignup,
                IsOpen = model.IsOpen(),
                HasClosed = model.HasClosed()
            };
        }
    }
}
