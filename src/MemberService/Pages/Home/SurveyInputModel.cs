namespace MemberService.Pages.Home;



using MemberService.Pages.Signup;

public class SurveyInputModel
{
    public IList<Answer> Answers { get; set; } = new List<Answer>();
}
