using Clave.Expressionify;
using MemberService.Data;

namespace MemberService.Pages.Home
{
    public class ClassSignupModel
    {
        public int Priority { get; set; }

        public string UserId { get; set; }

        public Status Status { get; set; }

        public DanceRole Role { get; set; }

        public string PartnerEmail { get; set; }

        [Expressionify]
        public static ClassSignupModel Create(EventSignup s) => new ClassSignupModel
        {
            Priority = s.Priority,
            UserId = s.UserId,
            Status = s.Status,
            Role = s.Role,
            PartnerEmail = s.PartnerEmail
        };
    }
}