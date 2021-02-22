using System;
using System.Collections.Generic;
using System.Linq;

using Clave.Expressionify;

namespace MemberService.Pages.AnnualMeeting.Survey
{
    public partial class SurveyModel
    {
        public Guid Id { get; set; }

        public Guid MeetingId { get; set; }

        public string Title { get; set; }

        public IReadOnlyList<QuestionModel> Questions { get; set; }

        [Expressionify]
        public static SurveyModel Create(Data.AnnualMeeting s) =>
        new()
        {
            Id = s.Survey.Id,
            MeetingId = s.Id,
            Title = s.Survey.Title,
            Questions = s.Survey.Questions
                .Select(q => QuestionModel.Create(s.Id, q))
                .ToList()
        };
    }
}