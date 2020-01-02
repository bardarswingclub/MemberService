using System.Linq;
using MemberService.Data;
using MemberService.Data.ValueTypes;

namespace MemberService.Pages.Event
{
    public class PartnerSignupModel
    {
        public PartnerSignupModel(string email)
        {
            Email = email;
        }

        public string Email { get; }

        public static PartnerSignupModel Create(string email, User partner)
        {
            if (partner?.EventSignups.FirstOrDefault() is EventSignup signup)
            {
                return new SignedUpPartnerSignupModel(signup);
            }

            if (partner is User)
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
        public KnownPartnerSignupModel(User partner) : base(partner.Email)
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
            PartnerEmail = signup.PartnerEmail?.Trim();
        }

        public DanceRole Role { get; }

        public Status Status { get; }

        public string PartnerEmail { get; }
    }
}