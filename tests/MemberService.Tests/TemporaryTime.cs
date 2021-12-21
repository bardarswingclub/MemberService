namespace MemberService.Tests;


public static class TemporaryTime
{
    public static IDisposable Is(DateTime utcNow) => new TemporaryDisposableTime(utcNow);

    private class TemporaryDisposableTime : IDisposable
    {
        public TemporaryDisposableTime(DateTime utcNow)
        {
            TimeProvider.UtcNowProvider = () => utcNow;
        }

        public void Dispose() => TimeProvider.Reset();
    }
}
