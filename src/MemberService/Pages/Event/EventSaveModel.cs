using System;
using System.Collections.Generic;
using MemberService.Data;

namespace MemberService.Pages.Event
{
    public class EventSaveModel
    {
        public Status Status { get; set; }

        public List<Item> Leads { get; set; } = new List<Item>();

        public List<Item> Follows { get; set; } = new List<Item>();

        public List<Item> Solos { get; set; } = new List<Item>();

        public class Item
        {
            public Guid Id { get; set; }

            public bool Selected { get; set; }
        }
    }
}
