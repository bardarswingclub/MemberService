using System;

namespace MemberService
{
    public static class Constants
    {
        public static DateTime ThisYear => new DateTime(DateTime.UtcNow.Year, 1, 1);

        public static DateTime ThisSemester => new DateTime(DateTime.UtcNow.Year, (DateTime.UtcNow.Month / 7) * 7 + 1, 1);
    }
}
