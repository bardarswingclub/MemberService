namespace MemberService.Data;

public class Survey
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public Semester Semester { get; set; }

    public Event Event { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();

    public ICollection<Response> Responses { get; set; } = new List<Response>();
}
