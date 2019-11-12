using System;
using System.Collections.Generic;

namespace MemberService.Data
{
    public class Question
    {
        public Guid Id { get; set; }

        public QuestionType Type { get; set; }

        public Guid EventId { get; set; }

        public Event Event { get; set; }

        public int Order { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}