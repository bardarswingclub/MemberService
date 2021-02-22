using System;
using System.Collections.Generic;
using System.Linq;

using Clave.Expressionify;

using MemberService.Data;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.AnnualMeeting.Survey
{
    public partial class QuestionModel
    {
        public Guid Id { get; set; }

        public Guid MeetingId { get; set; }

        public QuestionType Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyList<OptionModel> Options { get; set; }

        public DateTime? AnswerableFrom { get; set; }

        public DateTime? AnswerableUntil { get; set; }

        public bool VotingHasStarted => AnswerableFrom < TimeProvider.UtcNow;

        public bool VotingHasEnded => AnswerableUntil < TimeProvider.UtcNow;

        [Expressionify]
        public static QuestionModel Create(Guid meetingId, Question q)
            => new()
            {
                Id = q.Id,
                MeetingId = meetingId,
                Type = q.Type,
                Title = q.Title,
                Description = q.Description,
                AnswerableFrom = q.AnswerableFrom,
                AnswerableUntil = q.AnswerableUntil,
                Options = q.Options
                    .Select(o => OptionModel.Create(o))
                    .ToList()
            };
    }
}