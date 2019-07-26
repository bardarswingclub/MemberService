using System.Collections.Generic;

namespace MemberService.Pages.Event
{
    public class ParticipantsModel
    {
        public ParticipantsModel(IReadOnlyList<EventSignupModel> signups, bool allowPartnerSignup)
        {
            Signups = signups;
            AllowPartnerSignup = allowPartnerSignup;
        }

        public IReadOnlyList<EventSignupModel> Signups { get; }

        public EventSignupModel this[int index] => Signups[index];

        public bool AllowPartnerSignup { get; }
    }
}