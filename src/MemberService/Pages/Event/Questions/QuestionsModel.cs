using System;
using System.Collections.Generic;
using System.Linq;
using Clave.Expressionify;
using MemberService.Data;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Event.Questions
{
    public class QuestionsModel
    {
        public Guid EventId { get; set; }

        public string EventTitle { get; set; }

        public string EventDescription { get; set; }

        public string Filter { get; set; }

        public IReadOnlyList<EventSignup> Signups { get; set; }

        public int SignupsByAll => Signups.Count;

        public int SignupsByApproved => Signups.Count(s => s.Status == Status.Approved || s.Status == Status.AcceptedAndPayed);

        public int SignupsByPaid => Signups.Count(s => s.Status == Status.AcceptedAndPayed);

        public IReadOnlyList<QuestionModel> Questions { get; set; }

        [Expressionify]
        public static QuestionsModel Create(Data.Event e, string filter) =>
        new QuestionsModel
        {
            EventId = e.Id,
            EventTitle = e.Title,
            EventDescription = e.Description,
            Filter = filter,
            Signups = e.Signups.ToList(),
            Questions = e.Questions
                .Select(q => QuestionModel.Create(q))
                .ToList()
        };
    }
}