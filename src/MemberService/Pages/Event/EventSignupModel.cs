using System;
using Clave.Expressionify;
using MemberService.Data;

namespace MemberService.Pages.Event
{
    public class EventSignupModel
    {
        public Guid Id { get; set; }

        public string FullName { get; set; }

        public DateTime SignedUpAt { get; set; }

        public Status Status { get; set; }

        public DanceRole Role { get; set; }

        public bool Selected { get; set; }

        [Expressionify]
        public static EventSignupModel Create(EventSignup s)
            => new EventSignupModel
            {
                Id = s.Id,
                FullName = s.User.FullName,
                SignedUpAt = s.SignedUpAt,
                Status = s.Status,
                Role = s.Role
            };
    }
}
