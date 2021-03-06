﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MemberService.Data.ValueTypes;

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
        public User CreatedByUser { get; set; }

        public EventSignupOptions SignupOptions { get; set; } = new();

        public Guid? SurveyId { get; set; }

        public Survey Survey { get; set; }

        public Guid? SemesterId { get; set; }

        public Semester Semester { get; set; }

        public ICollection<EventSignup> Signups { get; set; } = new List<EventSignup>();

        public bool Archived { get; set; }

        public EventType Type { get; set; }

        public int LessonCount { get; set; }
    }
}
