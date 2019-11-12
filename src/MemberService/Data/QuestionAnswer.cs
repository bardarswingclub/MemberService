using System;

namespace MemberService.Data
{
    public class QuestionAnswer
    {
        public Guid Id { get; set; }

        public Guid OptionId { get; set; }

        public QuestionOption Option { get; set; }

        public Guid SignupId { get; set; }

        public EventSignup Signup { get; set; }
    }
}