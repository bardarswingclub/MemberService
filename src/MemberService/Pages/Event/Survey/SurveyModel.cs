using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;
using MemberService.Data;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Event.Survey
{
    public class SurveyModel : EventBaseModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyList<QuestionModel> Questions { get; set; }

        [Expressionify]
        public static SurveyModel Create(Data.Survey s) =>
        new SurveyModel
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            Questions = s.Questions
                .Select(q => QuestionModel.Create(q))
                .ToList()
        };
    }
}