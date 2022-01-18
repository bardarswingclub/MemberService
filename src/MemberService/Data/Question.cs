namespace MemberService.Data;




using MemberService.Data.ValueTypes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class Question : IEntityTypeConfiguration<Question>
{
    public Guid Id { get; set; }

    public QuestionType Type { get; set; }

    public Guid SurveyId { get; set; }

    public Survey Survey { get; set; }

    public int Order { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();

    public DateTime? AnswerableFrom { get; set; }

    public DateTime? AnswerableUntil { get; set; }

    public void Configure(EntityTypeBuilder<Question> question)
    {
        question
            .Property(q => q.Type)
            .HasEnumStringConversion();
    }
}
