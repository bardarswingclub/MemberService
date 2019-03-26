using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clave.Expressionify;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Data
{
    public static class Extensions
    {
        [Expressionify]
        public static bool HasPayedMembershipThisYear(this MemberUser user)
            => user.Payments.Any(p => p.PayedAt > Constants.ThisYear && p.IncludesMembership);

        [Expressionify]
        public static bool HasPayedTrainingThisSemester(this MemberUser user)
            => user.Payments.Any(p => p.PayedAt > Constants.ThisSemester && p.IncludesTraining);

        [Expressionify]
        public static bool HasPayedClassesThisSemester(this MemberUser user)
            => user.Payments.Any(p => p.PayedAt > Constants.ThisSemester && p.IncludesClasses);

        public static async Task<MemberUser> SingleUser(this IQueryable<MemberUser> users, string id)
            => await users.SingleOrDefaultAsync(user => user.Id == id);
    }
}