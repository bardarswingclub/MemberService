using System;
using System.Linq;
using Clave.Expressionify;
using MemberService.Data;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Signup
{
    public class EventModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public EventType Type { get; set; }

        public string Description { get; set; }

        public DateTime? OpensAt { get; set; }

        public EventSignup UserSignup { get; set; }

        [Expressionify]
        public static EventModel Create(Data.Event e, string userId)
            => new EventModel
            {
                Id = e.Id,
                Title = e.Title,
                Type = e.Type,
                Description = e.Description,
                OpensAt = e.SignupOptions.SignupOpensAt,
                UserSignup = e.Signups.FirstOrDefault(s => s.UserId == userId)
            };

    }
}
