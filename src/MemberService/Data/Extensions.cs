using System.Linq;
using System.Threading.Tasks;
using Clave.Expressionify;
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

        public static async Task<MemberUser> SingleUser(this IQueryable<MemberUser> users, string id)
            => await users.SingleOrDefaultAsync(user => user.Id == id);
    }
}