using NodaTime;
using System;

namespace MemberService
{
    public static class TimeProvider
    {
        public static Func<DateTime> UtcNowProvider { get; set; } = () => DateTime.UtcNow;

        public static DateTime UtcNow => UtcNowProvider();

        public static DateTimeZone TimeZoneOslo { get; } = DateTimeZoneProviders.Tzdb["Europe/Oslo"];

        public static DateTime ThisYearUtc => new DateTime(UtcNow.Year, 1, 1);

        public static DateTime ThisSemesterUtc => new DateTime(UtcNow.Year, (UtcNow.Month >= 7 ? 7 : 1), 1);
    }
}
