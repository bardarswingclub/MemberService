namespace MemberService.Data;




public class QuestionOption
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public Question Question { get; set; }

    public int Order { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public ICollection<QuestionAnswer> Answers { get; set; } = new List<QuestionAnswer>();
}
