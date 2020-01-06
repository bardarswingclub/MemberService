using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;
using MemberService.Data;

namespace MemberService.Pages.Event.Survey
{
    public class SurveyResultModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyCollection<QuestionModel> Questions { get; set; }

        public IReadOnlyCollection<ResponseModel> Responses { get; set; }

        [Expressionify]
        public static SurveyResultModel Create(Data.Survey s) =>
            new SurveyResultModel
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Questions = s.Questions
                    .Select(q => QuestionModel.Create(q))
                    .ToList(),
                Responses = s.Responses
                    .Select(r => ResponseModel.Create(r))
                    .ToList()
            };
    }

    public class ResponseModel
    {
        public string Name { get; set; }

        public string UserId { get; set; }
        public static ResponseModel Create(Response r) =>
            new ResponseModel
            {
                UserId = r.UserId,
                Name = r.User.FullName
            };

    }
}