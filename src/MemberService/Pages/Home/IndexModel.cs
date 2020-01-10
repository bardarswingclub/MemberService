using System.Collections.Generic;

namespace MemberService.Pages.Home
{
    public class IndexModel
    {
        public IReadOnlyCollection<CourseSignupModel> Signups { get; set; }
    }
}
