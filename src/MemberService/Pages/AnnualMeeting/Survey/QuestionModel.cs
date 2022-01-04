namespace MemberService.Pages.AnnualMeeting.Survey;

using System.ComponentModel;

using Clave.Expressionify;

using MemberService.Data;
using MemberService.Data.ValueTypes;

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

    public partial class OptionModel
    {
        public Guid Id { get; set; }

        [DisplayName("Svaralternativ")]
        public string Title { get; set; }

        [DisplayName("Beskrivelse")]
        public string Description { get; set; }

        [Expressionify]
        public static OptionModel Create(QuestionOption o)
            => new()
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description
            };
    }

}
