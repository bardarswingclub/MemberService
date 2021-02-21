using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;
using MemberService.Pages.Signup;

namespace MemberService.Pages.Home
{
    public partial class SurveyModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyList<SignupQuestion> Questions { get; set; } = new List<SignupQuestion>();

        public IReadOnlyList<Guid> SelectedOptions { get; set; } = new List<Guid>();

        [Expressionify]
        public static SurveyModel Create(Data.Semester s, string userId) =>
            new SurveyModel
            {
                Title = s.Title,
                Description = s.Survey.Description,
                Questions = s.Survey.Questions
                    .Select(q => SignupQuestion.Create(q))
                    .ToList(),
                SelectedOptions = s.Survey.Responses
                    .Where(r => r.UserId == userId)
                    .SelectMany(r => r.Answers)
                    .Select(a => a.OptionId)
                    .ToList()
            };
    }
}