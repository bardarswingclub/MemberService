using System;

namespace MemberService.Data
{
    public class QuestionAnswer
    {
        public Guid Id { get; set; }

        public Guid OptionId { get; set; }

        public QuestionOption Option { get; set; }

        public Guid EventSignupId { get; set; }

        public EventSignup EventSignup { get; set; }
    }
}