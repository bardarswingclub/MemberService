using System;
using System.Collections.Generic;
using MemberService.Data;

namespace MemberService.Pages.Event
{
    public class EventModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ICollection<EventSignupModel> Leads { get; set; }

        public ICollection<EventSignupModel> Follows { get; set; }

        public ICollection<EventSignupModel> Solos { get; set; }

        public EventSignupOptions Options { get; set; }
    }
}