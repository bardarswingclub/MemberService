using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Data
{
    public static class Extensions
    {
        public static DateTime ThisYear => new DateTime(DateTime.Now.Year, 1, 1);

        public static DateTime ThisSemester => new DateTime(DateTime.Now.Year, (DateTime.Now.Month / 7) * 7 + 1, 1);

        public static readonly Expression<Func<MemberUser, bool>> HasPayedMembershipThisYearExpression = user
        => user.Payments.Any(p => p.PayedAt > ThisYear
            && p.IncludesMembership
            && !p.Refunded);

        public static readonly Func<MemberUser, bool> HasPayedMembershipThisYearFunc = HasPayedMembershipThisYearExpression.Compile();

        public static readonly Expression<Func<MemberUser, bool>> HasPayedTrainingFeeThisSemesterExpression = user
        => user.Payments.Any(p => p.PayedAt > ThisSemester
            && p.IncludesTraining
            && !p.Refunded);

        public static readonly Func<MemberUser, bool> HasPayedTrainingFeeThisSemesterFunc = HasPayedTrainingFeeThisSemesterExpression.Compile();

        public static readonly Expression<Func<MemberUser, bool>> HasPayedClassesFeeThisSemesterExpression = user
        => user.Payments.Any(p => p.PayedAt > ThisSemester
            && p.IncludesClasses
            && !p.Refunded);

        public static readonly Func<MemberUser, bool> HasPayedClassesFeeThisSemesterFunc = HasPayedClassesFeeThisSemesterExpression.Compile();

        public static bool HasPayedMembershipThisYear(this MemberUser user) => HasPayedMembershipThisYearFunc(user);

        public static bool HasPayedTrainingFeeThisSemester(this MemberUser user) => HasPayedTrainingFeeThisSemesterFunc(user);

        public static bool HasPayedClassesFeeThisSemester(this MemberUser user) => HasPayedClassesFeeThisSemesterFunc(user);

        public static async Task<MemberUser> SingleUser(this IQueryable<MemberUser> users, string id)
            => await users.SingleOrDefaultAsync(user => user.Id == id);
    }
}