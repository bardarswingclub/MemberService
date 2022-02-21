namespace MemberService.Pages.Home;

public class SignupModel
{
    public IReadOnlyCollection<CourseModel> Courses { get; set; }

    public IReadOnlyCollection<CourseModel> OpenedClasses { get; set; }

    public DateTime? OpensAt { get; set; }

    public bool Sortable { get; set; }

    public string SignupHelpText { get; set; }
}
