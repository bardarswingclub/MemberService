using MemberService.Data;
using System.ComponentModel;

namespace MemberService.Pages.Signup
{
    public class SignupInputModel
    {
        [DisplayName("Danserolle")]
        public DanceRole Role { get; set; }

        [DisplayName("Partners e-post")]
        public string PartnerEmail { get; set; }
    }
}