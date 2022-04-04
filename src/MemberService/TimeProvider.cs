namespace MemberService;

using NodaTime;

public static class TimeProvider
{
    public static DateTimeZone TimeZoneOslo { get; } = DateTimeZoneProviders.Tzdb["Europe/Oslo"];

    public static DateTime ThisYearUtc => UtcNow.GetStartOfYear();

    public static DateTime LastYearUtc => new(UtcNow.Year - 1, 1, 1);

    public static DateTime ThisSemesterUtc => UtcNow.GetStartOfSemester();

    public static DateTime NextSemesterUtc => UtcNow.GetStartOfNextSemester();

    public static DateTime UtcNow => UtcNowProvider();

    public static DateTime UtcToday => UtcNow.Date;

    public static Func<DateTime> UtcNowProvider { get; set; } = DefaultProvider;

    public static void Reset() => UtcNowProvider = DefaultProvider;

    private static DateTime DefaultProvider() => DateTime.UtcNow;
}
