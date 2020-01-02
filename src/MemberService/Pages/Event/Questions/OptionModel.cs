using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using Clave.Expressionify;
using MemberService.Data;
using MemberService.Data.ValueTypes;

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

        public IReadOnlyList<AnswerModel> SelectedByAll => SelectedBy;

        public IReadOnlyList<AnswerModel> SelectedByApproved => SelectedBy.Where(s => s.Status == Status.AcceptedAndPayed || s.Status == Status.Approved).ToList();

        public IReadOnlyList<AnswerModel> SelectedByPaid => SelectedBy.Where(s => s.Status == Status.AcceptedAndPayed).ToList();

        [Expressionify]
        public static OptionModel Create(QuestionOption o)
            => new OptionModel
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                SelectedBy = o.Answers
                    .Select(a => AnswerModel.Create(a.EventSignup))
                    .ToList()
            };
    }
}