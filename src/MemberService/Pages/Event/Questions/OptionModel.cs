using System;
using Clave.Expressionify;
using MemberService.Data;

namespace MemberService.Pages.Event.Questions
{
    public class OptionModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        [Expressionify]
        public static OptionModel Create(QuestionOption o)
            => new OptionModel
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description
            };
    }
}