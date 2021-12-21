namespace MemberService.Data;




public class Semester
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public DateTime SignupOpensAt { get; set; }

    public Guid? SurveyId { get; set; }

    public Survey Survey { get; set; }

    public ICollection<Event> Courses { get; set; } = new List<Event>();

    public string SignupHelpText { get; set; }
}
