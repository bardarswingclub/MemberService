using System;
using System.Collections.Generic;
using MemberService.Data;
using System.ComponentModel;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Signup
{
    public class SignupInputModel
    {
        [DisplayName("Danserolle")]
        public DanceRole Role { get; set; }

        [DisplayName("Partners e-post")]
        public string PartnerEmail { get; set; }

        public IList<Answer> Answers { get; set; } = new List<Answer>();

        public class Answer
        {
            public Guid QuestionId { get; set; }

            public IList<Guid> Selected { get; set; } = new List<Guid>();
        }
    }
}