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

        public static readonly Expression<Func<MemberUser, bool>> HasPayedMembershipThisYearExpression = user => user.Payments.Any(p => p.PayedAt > ThisYear && p.IncludesMembership);

        public static readonly Func<MemberUser, bool> HasPayedMembershipThisYearFunc = HasPayedMembershipThisYearExpression.Compile();

        public static readonly Expression<Func<MemberUser, bool>> HasPayedTrainingThisSemesterExpression = user => user.Payments.Any(p => p.PayedAt > ThisSemester && p.IncludesTraining);

        public static readonly Func<MemberUser, bool> HasPayedTrainingThisSemesterFunc = HasPayedTrainingThisSemesterExpression.Compile();

        public static readonly Expression<Func<MemberUser, bool>> HasPayedClassesThisSemesterExpression = user => user.Payments.Any(p => p.PayedAt > ThisSemester && p.IncludesClasses);

        public static readonly Func<MemberUser, bool> HasPayedClassesThisSemesterFunc = HasPayedClassesThisSemesterExpression.Compile();

        public static bool HasPayedMembershipThisYear(this MemberUser user) => HasPayedMembershipThisYearFunc(user);

        public static bool HasPayedTrainingThisSemester(this MemberUser user) => HasPayedTrainingThisSemesterFunc(user);

        public static bool HasPayedClassesThisSemester(this MemberUser user) => HasPayedClassesThisSemesterFunc(user);

        public static async Task<MemberUser> SingleUser(this IQueryable<MemberUser> users, string id)
            => await users.SingleOrDefaultAsync(user => user.Id == id);
    }
}