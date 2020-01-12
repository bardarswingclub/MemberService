using System;
using System.Collections.Generic;
using MemberService.Pages.Signup;

namespace MemberService.Pages.Home
{
    public class IndexModel
    {
        public DateTime? SignupOpensAt { get; set; }

        public IReadOnlyCollection<CourseSignupModel> Signups { get; set; } = new List<CourseSignupModel>();

        public string UserId { get; set; }

        public EventsModel PartyModel { get; set; }

        public EventsModel WorkshopModel { get; set; }
    }
}
