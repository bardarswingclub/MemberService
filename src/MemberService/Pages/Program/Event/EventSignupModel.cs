using System;
using System.Collections.Generic;
using Clave.ExtensionMethods;
using MemberService.Data;

namespace MemberService.Pages.Program.Event
{
    public class EventSignupModel
    {
        public Guid Id { get; private set; }

        public string UserId { get; private set; }

        public string FullName { get; private set; }

        public string Email { get; private set; }

        public DateTime SignedUpAt { get; private set; }

        public Status Status { get; private set; }

        public DanceRole Role { get; private set; }

        public bool Selected { get; private set; }

        public IReadOnlyCollection<EventSignupAuditEntry> AuditLog { get; private set; }

        public PartnerSignupModel Partner { get; private set; }

        public static EventSignupModel Create(EventSignup s)
            => new EventSignupModel
            {
                Id = s.Id,
                UserId = s.UserId,
                FullName = s.User.FullName,
                Email = s.User.NormalizedEmail,
                SignedUpAt = s.SignedUpAt,
                Status = s.Status,
                Partner = PartnerSignupModel.Create(s.PartnerEmail, s.Partner),
                Role = s.Role,
                AuditLog = s.AuditLog
                    .ToReadOnlyCollection()
            };
    }
}
