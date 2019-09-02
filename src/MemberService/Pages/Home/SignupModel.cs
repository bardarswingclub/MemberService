using System;
using System.Collections.Generic;

namespace MemberService.Pages.Home
{
    public class SignupModel
    {
        public IReadOnlyCollection<ClassModel> Classes { get; set; }

        public IReadOnlyCollection<ClassModel> OpenClasses { get; set; }

        public DateTime? OpensAt { get; set; }

        public bool Sortable { get; set; }
    }
}
