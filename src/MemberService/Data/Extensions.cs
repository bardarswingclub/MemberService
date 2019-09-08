using System;
using System.Linq;
using System.Threading.Tasks;
using Clave.Expressionify;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MemberService.Data
{
    public static class Extensions
    {
        [Expressionify]
        public static bool HasPayedMembershipThisYear(this MemberUser user)
            => user.Payments.Any(p => p.PayedAtUtc > TimeProvider.ThisYearUtc && p.IncludesMembership && !p.Refunded);

        [Expressionify]
        public static bool HasPayedTrainingFeeThisSemester(this MemberUser user)
            => user.Payments.Any(p => p.PayedAtUtc > TimeProvider.ThisSemesterUtc && p.IncludesTraining && !p.Refunded)
            || user.ExemptFromTrainingFee && user.HasPayedMembershipThisYear();

        [Expressionify]
        public static bool HasPayedClassesFeeThisSemester(this MemberUser user)
            => user.Payments.Any(p => p.PayedAtUtc > TimeProvider.ThisSemesterUtc && p.IncludesClasses && !p.Refunded)
            || user.ExemptFromClassesFee && user.HasPayedTrainingFeeThisSemester();

        [Expressionify]
        public static bool HasOpened(this Event e)
            => e.SignupOptions.SignupOpensAt == null || e.SignupOptions.SignupOpensAt < TimeProvider.UtcNow;

        [Expressionify]
        public static bool HasClosed(this Event e)
            => e.SignupOptions.SignupClosesAt != null && e.SignupOptions.SignupClosesAt < TimeProvider.UtcNow;

        [Expressionify]
        public static bool IsOpen(this Event e)
            => e.HasOpened() && !e.HasClosed();

        [Expressionify]
        public static bool WillOpen(this Event e)
            => !e.HasOpened() && !e.HasClosed();

        [Expressionify]
        public static bool IsSignedUpFor(this MemberUser user, Guid id)
            => user.EventSignups.Any(e => e.EventId == id);

        public static async Task<MemberUser> SingleUser(this IQueryable<MemberUser> users, string id)
            => await users.SingleOrDefaultAsync(user => user.Id == id);

        public static void HasEnumStringConversion<TEnum>(this PropertyBuilder<TEnum> property)
            where TEnum : struct
            => property.HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<TEnum>(v));
    }
}