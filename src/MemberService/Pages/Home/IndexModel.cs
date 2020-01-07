using System.Collections.Generic;

namespace MemberService.Pages.Home
{
    public class IndexModel
    {
        public IReadOnlyCollection<ClassSignupModel> Signups { get; set; }
    }
}
