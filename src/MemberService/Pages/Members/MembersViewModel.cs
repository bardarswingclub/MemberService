using System.Collections.Generic;
using MemberService.Data;

namespace MemberService.Pages.Members
{
    public class MembersViewModel
    {
        public IReadOnlyCollection<(char Letter, IReadOnlyCollection<MemberUser> Users)> Users { get; set; }

        public string MemberFilter { get; set; }

        public string TrainingFilter { get; set; }

        public string ClassesFilter { get; set; }
    }
}