using System.Collections.Generic;
using MemberService.Data;

namespace MemberService.Pages.Event
{
    public class ParticipantsModel
    {
        public ParticipantsModel(EventSignupOptions options, IReadOnlyList<EventSignupModel> signups)
        {
            Options = options;
            Signups = signups;
        }

        public EventSignupOptions Options { get; }

        public IReadOnlyList<EventSignupModel> Signups { get; }

        public EventSignupModel this[int index] => Signups[index];
    }
}