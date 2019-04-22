using NodaTime;
using System;
using System.Linq.Expressions;

namespace MemberService.Services
{
    public static class Extensions
    {
        public static string FormatMoney(this decimal amount)
            => string.Format("kr {0:0},-", amount);

        public static string ToOsloDate(this DateTime utc)
            => Instant.FromDateTimeUtc(utc.WithKind(DateTimeKind.Utc)).InZone(Constants.TimeZoneOslo).Date.ToString();

        public static string ToOsloDateTime(this DateTime utc)
            => Instant.FromDateTimeUtc(utc.WithKind(DateTimeKind.Utc)).InZone(Constants.TimeZoneOslo).ToString();

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> predicate)
            => Expression.Lambda<Func<T, bool>>(Expression.Not(predicate.Body), predicate.Parameters);

        private static DateTime WithKind(this DateTime dateTime, DateTimeKind kind)
            => DateTime.SpecifyKind(dateTime, kind);
    }
}