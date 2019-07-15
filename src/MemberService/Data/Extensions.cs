using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clave.Expressionify;
using MemberService.Pages.Event;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Data
{
    public static class Extensions
    {
        [Expressionify]
        public static bool HasPayedMembershipThisYear(this MemberUser user)
            => user.Payments.Any(p => p.PayedAtUtc > Constants.ThisYearUtc && p.IncludesMembership && !p.Refunded);

        [Expressionify]
        public static bool HasPayedTrainingFeeThisSemester(this MemberUser user)
            => user.Payments.Any(p => p.PayedAtUtc > Constants.ThisSemesterUtc && p.IncludesTraining && !p.Refunded)
            || user.ExemptFromTrainingFee && user.HasPayedMembershipThisYear();

        [Expressionify]
        public static bool HasPayedClassesFeeThisSemester(this MemberUser user)
            => user.Payments.Any(p => p.PayedAtUtc > Constants.ThisSemesterUtc && p.IncludesClasses && !p.Refunded)
            || user.ExemptFromClassesFee && user.HasPayedTrainingFeeThisSemester();

        [Expressionify]
        public static bool HasOpened(this EventSignupOptions x)
            => x.SignupOpensAt == null || x.SignupOpensAt < DateTime.UtcNow;

        [Expressionify]
        public static bool HasClosed(this EventSignupOptions x)
            => x.SignupClosesAt != null && x.SignupClosesAt < DateTime.UtcNow;

        [Expressionify]
        public static bool IsOpen(this EventSignupOptions x)
            => x.HasOpened() && !x.HasClosed();

        public static async Task<MemberUser> SingleUser(this IQueryable<MemberUser> users, string id)
            => await users.SingleOrDefaultAsync(user => user.Id == id);
    }
}