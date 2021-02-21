using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;

namespace MemberService.Pages.Semester.Survey
{
    public class SurveyModel
    {
        public Guid Id { get; set; }

        public Guid SemesterId { get; set; }

        public string Title { get; set; }

        public IReadOnlyList<QuestionModel> Questions { get; set; }

        [Expressionify]
        public static SurveyModel Create(Data.Semester s) =>
        new SurveyModel
        {
            Id = s.Survey.Id,
            SemesterId = s.Id,
            Title = s.Survey.Title,
            Questions = s.Survey.Questions
                .Select(q => QuestionModel.Create(q))
                .ToList()
        };
    }
}