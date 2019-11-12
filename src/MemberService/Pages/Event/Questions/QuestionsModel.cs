using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;

namespace MemberService.Pages.Event.Questions
{
    public class QuestionsModel
    {
        public Guid EventId { get; set; }

        public string EventTitle { get; set; }

        public string EventDescription { get; set; }

        public IReadOnlyList<QuestionModel> Questions { get; set; }

        [Expressionify]
        public static QuestionsModel Create(Data.Event e) =>
        new QuestionsModel
        {
            EventId = e.Id,
            EventTitle = e.Title,
            EventDescription = e.Description,
            Questions = e.Questions
                .Select(q => QuestionModel.Create(q))
                .ToList()
        };
    }
}