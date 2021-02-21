using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Clave.Expressionify;
using MemberService.Data;

namespace MemberService.Pages.Event.Survey
{
    public partial class SurveyResultModel : EventBaseModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Filter { get; set; }

        public IReadOnlyList<QuestionModel> Questions { get; set; }

        public IReadOnlyCollection<ResponseModel> Responses { get; set; }

        [Expressionify]
        public static SurveyResultModel Create(Data.Event e, string filter, Expression<Func<EventSignup, bool>> filterExpression) =>
            new()
            {
                Id = e.Survey.Id,
                EventId = e.Id,
                SemesterId = e.SemesterId,
                EventTitle = e.Title,
                EventDescription = e.Description,
                Title = e.Survey.Title,
                Filter = filter,
                Description = e.Survey.Description,
                Questions = e.Survey.Questions
                    .Select(q => QuestionModel.Create(q))
                    .ToList(),
                Responses = e.Signups.AsQueryable()
                    .Where(filterExpression)
                    .SelectMany(es => es.Response.Answers.Select(a => ResponseModel.Create(es, a)))
                    .ToList()
            };

    }
}