namespace MemberService.Pages.Signup;

using System.ComponentModel;

using MemberService.Data.ValueTypes;

public class SignupInputModel
{
    [DisplayName("Danserolle")]
    public DanceRole Role { get; set; }

    [DisplayName("Partners e-post")]
    public string PartnerEmail { get; set; }

    [DisplayName("Hvilken by/land kommer du fra?")]
    public string Origin { get; set; }

    [DisplayName("Andre behov, for eksempel allergier")]
    public string SpecialNeeds { get; set; }

    public IList<Answer> Answers { get; set; } = new List<Answer>();
}
