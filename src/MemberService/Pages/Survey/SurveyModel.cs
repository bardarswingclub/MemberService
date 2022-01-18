namespace MemberService.Pages.Survey;

using System.ComponentModel;

using Clave.Expressionify;

using MemberService.Data;
using MemberService.Data.ValueTypes;

public partial class SurveyModel
{
    public Guid Id { get; set; }

    public Guid? SemesterId { get; set; }

    public Guid? EventId { get; set; }

    public string Title { get; set; }

    public IReadOnlyList<QuestionModel> Questions { get; set; }

    [Expressionify]
    public static SurveyModel Create(Survey s) =>
    new()
    {
        Id = s.Id,
        SemesterId = s.Semester.Id,
        EventId = s.Event.Id,
        Title = s.Title,
        Questions = s.Questions
            .Select(q => QuestionModel.Create(q))
            .ToList()
    };

    public partial class QuestionModel
    {
        public Guid Id { get; set; }

        public QuestionType Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public IReadOnlyList<OptionModel> Options { get; set; }

        [Expressionify]
        public static QuestionModel Create(Question q)
            => new()
            {
                Id = q.Id,
                Type = q.Type,
                Title = q.Title,
                Description = q.Description,
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
}
