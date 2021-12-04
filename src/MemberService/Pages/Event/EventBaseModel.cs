using System;
using System.ComponentModel;

namespace MemberService.Pages.Event
{
    public abstract class EventBaseModel
    {
        public Guid EventId { get; set; }

        public Guid? SemesterId { get; set; }

        [DisplayName("Navn")]
        public string EventTitle { get; set; }

        [DisplayName("Beskrivelse")]
        public string EventDescription { get; set; }

        public bool IsArchived { get; set; }

        public bool IsCancelled { get; set; }
    }
}