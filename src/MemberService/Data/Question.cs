using System;
using System.Collections.Generic;
using MemberService.Data.ValueTypes;

namespace MemberService.Data
{
    public class Question
    {
        public Guid Id { get; set; }

        public QuestionType Type { get; set; }

        public Guid SurveyId { get; set; }

        public Survey Survey { get; set; }

        public int Order { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();

        public DateTime? AnswerableFrom { get; set; }

        public DateTime? AnswerableUntil { get; set; }
    }
}