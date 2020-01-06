using System;
using System.ComponentModel;

namespace MemberService.Pages.Event
{
    public abstract class EventBaseModel
    {
        public Guid EventId { get; set; }

        [DisplayName("Navn")]
        public string EventTitle { get; set; }

        [DisplayName("Beskrivelse")]
        public string EventDescription { get; set; }
    }
}