using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Event
{
    public class EventSaveModel
    {
        [Required]
        public Status Status { get; set; }

        public List<Item> Leads { get; set; } = new List<Item>();

        public List<Item> Follows { get; set; } = new List<Item>();

        public List<Item> Solos { get; set; } = new List<Item>();

        public bool SendEmail { get; set; }

        public bool ReplyToMe { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public class Item
        {
            public Guid Id { get; set; }

            public bool Selected { get; set; }
        }
    }
}
