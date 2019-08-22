using System.Linq;
using MemberService.Data;

namespace MemberService.Pages.Program.Event
{
    public class PartnerSignupModel
    {
        public PartnerSignupModel(string email)
        {
            Email = email;
        }

        public string Email { get; }

        public static PartnerSignupModel Create(string email, MemberUser partner)
        {
            if (partner?.EventSignups.FirstOrDefault() is EventSignup signup)
            {
                return new SignedUpPartnerSignupModel(signup);
            }

            if (partner is MemberUser)
            {
                return new KnownPartnerSignupModel(partner);
            }

            if (email is string)
            {
                return new PartnerSignupModel(email);
            }

            return null;
        }
    }

    public class KnownPartnerSignupModel : PartnerSignupModel
    {
        public KnownPartnerSignupModel(MemberUser partner) : base(partner.Email)
        {
            Id = partner?.Id;
            Name = partner?.FullName;
        }

        public string Id { get; }

        public string Name { get; }

    }

    public class SignedUpPartnerSignupModel : KnownPartnerSignupModel
    {
        public SignedUpPartnerSignupModel(EventSignup signup) : base(signup.User)
        {
            Role = signup.Role;
            Status = signup.Status;
            PartnerEmail = signup.PartnerEmail;
        }

        public DanceRole Role { get; }

        public Status Status { get; }

        public string PartnerEmail { get; }
    }
}