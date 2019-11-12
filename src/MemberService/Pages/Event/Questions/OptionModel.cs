using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Clave.Expressionify;
using MemberService.Data;

namespace MemberService.Pages.Event.Questions
{
    public class OptionModel
    {
        public Guid Id { get; set; }

        [DisplayName("Svaralternativ")]
        public string Title { get; set; }

        [DisplayName("Beskrivelse")]
        public string Description { get; set; }

        public IReadOnlyList<AnswerModel> SelectedBy { get; set; }

        [Expressionify]
        public static OptionModel Create(QuestionOption o)
            => new OptionModel
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                SelectedBy = o.Answers
                    .Select(a => AnswerModel.Create(a.Signup))
                    .ToList()
            };
    }
}