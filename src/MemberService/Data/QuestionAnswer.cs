namespace MemberService.Data;



public class QuestionAnswer
{
    public Guid Id { get; set; }

    public Guid OptionId { get; set; }

    public QuestionOption Option { get; set; }

    public Guid ResponseId { get; set; }

    public Response Response { get; set; }

    public DateTime? AnsweredAt { get; set; }
}
