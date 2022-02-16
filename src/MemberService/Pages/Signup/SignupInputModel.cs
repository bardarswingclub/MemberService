namespace MemberService.Pages.Signup;

using System.ComponentModel;

using MemberService.Data.ValueTypes;

public class SignupInputModel
{
    [DisplayName("Danserolle")]
    public DanceRole Role { get; set; }

    [DisplayName("Partners e-post")]
    public string PartnerEmail { get; set; }

    public IList<Answer> Answers { get; set; } = new List<Answer>();
}
