using NodaTime;

using System;

namespace MemberService
{
    public static class TimeProvider
    {
        public static DateTimeZone TimeZoneOslo { get; } = DateTimeZoneProviders.Tzdb["Europe/Oslo"];

        public static DateTime ThisYearUtc => new(UtcNow.Year, 1, 1);

        public static DateTime LastYearUtc => new(UtcNow.Year - 1, 1, 1);

        public static DateTime ThisSemesterUtc => new(UtcNow.Year, (UtcNow.Month >= 7 ? 7 : 1), 1);

        public static DateTime NextSemesterUtc => new DateTime(UtcNow.Year, (UtcNow.Month >= 7 ? 7 : 1), 1).AddMonths(6);

        public static DateTime UtcNow => UtcNowProvider();

        public static DateTime UtcToday => UtcNow.Date;

        public static Func<DateTime> UtcNowProvider { get; set; } = DefaultProvider;

        public static void Reset() => UtcNowProvider = DefaultProvider;

        private static DateTime DefaultProvider() => DateTime.UtcNow;
    }
}
