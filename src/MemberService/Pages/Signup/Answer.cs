namespace MemberService.Pages.Signup;




public class Answer
{
    public Guid QuestionId { get; set; }

    public IList<Guid> Selected { get; set; } = new List<Guid>();
}
