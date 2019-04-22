using System;
using System.Linq.Expressions;

namespace MemberService.Services
{
    public static class Extensions
    {
        public static string FormatMoney(this decimal amount) => string.Format("kr {0:0},-", amount);

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> predicate)
            => Expression.Lambda<Func<T, bool>>(Expression.Not(predicate.Body), predicate.Parameters);
    }
}