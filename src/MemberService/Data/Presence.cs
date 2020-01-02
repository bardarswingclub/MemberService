using System;

namespace MemberService.Data
{
    public class Presence
    {
        public Guid Id { get; set; }

        public Guid SignupId { get; set; }

        public EventSignup Signup { get; set; }

        public int Lesson { get; set; }

        public bool Present { get; set; }

        public DateTime RegisteredAt { get; set; }

        public string RegisteredById { get; set; }

        public User RegisteredBy { get; set; }
    }
}