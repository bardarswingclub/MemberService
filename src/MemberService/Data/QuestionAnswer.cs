using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemberService.Data
{
    public class QuestionAnswer
    {
        public Guid Id { get; set; }

        public Guid OptionId { get; set; }

        [ForeignKey(nameof(OptionId))]
        public QuestionOption Option { get; set; }

        public Guid SignupId { get; set; }

        [ForeignKey(nameof(SignupId))]
        public EventSignup Signup { get; set; }
    }
}