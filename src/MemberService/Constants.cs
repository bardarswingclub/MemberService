using NodaTime;
using System;

namespace MemberService
{
    public static class Constants
    {
        public static DateTimeZone TimeZoneOslo { get; } = DateTimeZoneProviders.Tzdb["Europe/Oslo"];

        public static DateTime ThisYearUtc => new DateTime(DateTime.UtcNow.Year, 1, 1);

        public static DateTime ThisSemesterUtc => new DateTime(DateTime.UtcNow.Year, (DateTime.UtcNow.Month >= 7 ? 7 : 1), 1);
    }
}
