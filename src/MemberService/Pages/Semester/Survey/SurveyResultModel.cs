using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Clave.Expressionify;

namespace MemberService.Pages.Semester.Survey
{
    public class SurveyResultModel
    {
        public Guid Id { get; set; }

        public Guid SemesterId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Filter { get; set; }

        public IReadOnlyList<QuestionModel> Questions { get; set; }

        public IReadOnlyCollection<ResponseModel> Responses { get; set; }

        [Expressionify]
        public static SurveyResultModel Create(Data.Semester s, string filter, Expression<Func<ResponseModel, bool>> filterExpression) =>
            new SurveyResultModel
            {
                Id = s.Survey.Id,
                SemesterId = s.Id,
                Title = s.Survey.Title,
                Filter = filter,
                Description = s.Survey.Description,
                Questions = s.Survey.Questions
                    .Select(q => QuestionModel.Create(q))
                    .ToList(),
                Responses = s.Survey.Responses.AsQueryable()
                    .SelectMany(r => r.Answers.Select(a => ResponseModel.Create(r, a)))
                    .Where(filterExpression)
                    .ToList()
            };

    }
}