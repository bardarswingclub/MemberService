using System.Collections.Generic;
using MemberService.Pages.Signup;

namespace MemberService.Pages.Home
{
    public class SurveyInputModel
    {
        public IList<Answer> Answers { get; set; } = new List<Answer>();
    }
}