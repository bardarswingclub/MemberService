using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clave.Expressionify;
using MemberService.Data;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace MemberService.Pages.Signup
{
    public static class Logic
    {
        public static async Task<IReadOnlyList<EventModel>> GetEvents(this MemberContext db, string userId, Expression<Func<Data.Event, bool>> predicate)
            => await db.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Where(e => e.Archived == false)
                .Where(predicate)
                .OrderBy(e => e.SignupOptions.SignupOpensAt)
                .Select(e => EventModel.Create(e, userId))
                .ToListAsync();

        public static async Task<SignupModel> GetSignupModel(this MemberContext db, Guid id)
            => await db.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Where(e => e.Archived == false)
                .Where(e => e.IsOpen())
                .Select(e => SignupModel.Create(e))
                .SingleOrDefaultAsync(e => e.Id == id);

        public static async Task<Data.Event> GetEditableEvent(this MemberContext db, Guid id)
            => await db.Events
                .Include(e => e.Signups)
                .Include(e => e.SignupOptions)
                .SingleOrDefaultAsync(e => e.Id == id);

        public static async Task<MemberUser> GetUser(this MemberContext database, string userId)
            => await database.Users
                .Include(u => u.Payments)
                .Include(u => u.EventSignups)
                .AsNoTracking()
                .SingleUser(userId);

        public static async Task<MemberUser> GetEditableUser(this MemberContext database, string userId)
            => await database.Users
                .Include(u => u.Payments)
                .Include(u => u.EventSignups)
                .SingleUser(userId);

        public static EventSignup GetEditableEvent(this MemberUser user, Guid id)
            => user.EventSignups
                .Where(e => e.CanEdit())
                .FirstOrDefault(e => e.EventId == id);

        public static bool ShouldAutoAccept(this Data.Event model, DanceRole role)
            => model.SignupOptions.AutoAcceptedSignups > model.Signups.Count(s => s.Role == role);

        public static void AddEventSignup(this MemberUser user, Guid id, SignupInputModel input, bool autoAccept)
        {
            var status = autoAccept ? Status.Approved : Status.Pending;
            user.EventSignups.Add(new EventSignup
            {
                EventId = id,
                Priority = 1,
                Role = input.Role,
                PartnerEmail = input.PartnerEmail?.Normalize().ToUpperInvariant(),
                Status = status,
                SignedUpAt = DateTime.UtcNow,
                AuditLog =
                {
                    {
                        $"Signed up as {input.Role}{(input.PartnerEmail is string partnerEmail ? $" with partner {partnerEmail}" : "")}, status is {status}",
                        user
                    }
                }
            });
        }

        public static bool CanEdit(this EventSignup e)
            => e.Status == Status.Pending || e.Status == Status.Recommended;

        public static bool MustPayNonMembersPrice(this MemberUser user, EventSignupOptions options)
            => options.PriceForNonMembers > 0 && !user.HasPayedMembershipThisYear();

        public static bool MustPayMembersPrice(this MemberUser user, EventSignupOptions options)
            => options.PriceForMembers > 0 && user.HasPayedMembershipThisYear();

        public static bool MustPayMembershipFee(this MemberUser user, EventSignupOptions options)
            => options.RequiresMembershipFee && !user.HasPayedMembershipThisYear();

        public static bool MustPayTrainingFee(this MemberUser user, EventSignupOptions options)
            => options.RequiresTrainingFee && !user.HasPayedTrainingFeeThisSemester();

        public static bool MustPayClassesFee(this MemberUser user, EventSignupOptions options)
            => options.RequiresClassesFee && !user.HasPayedClassesFeeThisSemester();
    }
}