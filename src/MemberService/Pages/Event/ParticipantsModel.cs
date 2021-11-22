using System.Collections.Generic;

namespace MemberService.Pages.Event
{
    public class ParticipantsModel
    {
        public ParticipantsModel(IReadOnlyList<EventSignupModel> signups, bool allowPartnerSignup, bool showPriority)
        {
            Signups = signups;
            AllowPartnerSignup = allowPartnerSignup;
            ShowPriority = showPriority;
        }

        public IReadOnlyList<EventSignupModel> Signups { get; }

        public EventSignupModel this[int index] => Signups[index];

        public bool AllowPartnerSignup { get; }

        public bool ShowPriority { get; }
    }
}