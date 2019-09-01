using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemberService.Pages.Home
{
    public class IndexModel
    {
        public IReadOnlyCollection<ClassSignupModel> Signups { get; set; }
    }
}
