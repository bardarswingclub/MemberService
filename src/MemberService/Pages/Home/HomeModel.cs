using System;
using System.Collections.Generic;

namespace MemberService.Pages.Home
{
    public class HomeModel
    {
        public IReadOnlyCollection<Signup.EventModel> Classes { get; set; }

        public DateTime? OpensAt { get; set; }
    }
}
