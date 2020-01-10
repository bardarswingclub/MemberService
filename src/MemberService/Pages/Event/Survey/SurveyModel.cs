using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;

namespace MemberService.Pages.Event.Survey
{
    public class SurveyModel : EventBaseModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyList<QuestionModel> Questions { get; set; }

        [Expressionify]
        public static SurveyModel Create(Data.Event e) =>
        new SurveyModel
        {
            Id = e.Survey.Id,
            EventId = e.Id,
            EventTitle = e.Title,
            EventDescription = e.Description,
            Title = e.Survey.Title,
            Description = e.Survey.Description,
            Questions = e.Survey.Questions
                .Select(q => QuestionModel.Create(q))
                .ToList()
        };
    }
}