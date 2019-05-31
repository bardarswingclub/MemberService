﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberService.Data
{
    public class Event
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public MemberUser CreatedByUser { get; set; }

        public EventSignupOptions SignupOptions { get; set; }

        public ICollection<EventSignup> Signups { get; set; } = new List<EventSignup>();

        public bool Archived { get; set; }
    }
}