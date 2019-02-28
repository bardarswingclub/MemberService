using System.Collections.Generic;
using MemberService.Data;

namespace MemberService.Pages.Members
{
    public class MembersViewModel
    {
        public IReadOnlyCollection<MemberUser> Users { get; set; }

        public bool OnlyMembers { get; set; }

        public bool OnlyTraining { get; set; }

        public bool OnlyClasses { get; set; }
    }
}