using System;
using System.Collections.Generic;
using MemberService.Data;

namespace MemberService.Pages.Event.Presence
{
    public class ParticipantsModel
    {
        public ParticipantsModel(EventSignup s, int count)
        {
            Id = s.Id;
            UserId = s.UserId;
            FullName = s.User.FullName;
            Presence = CreateList(s.Presence, count);
        }

        public Guid Id { get; }

        public string UserId { get; }

        public string FullName { get; }

        public IReadOnlyList<bool> Presence { get; }

        private static IReadOnlyList<bool> CreateList(IEnumerable<Data.Presence> presence, int count)
        {
            var list = new bool[count];
            foreach (var p in presence)
            {
                list[p.Lesson] = p.Present;
            }

            return list;
        }
    }
}