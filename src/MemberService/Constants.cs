using System;

namespace MemberService
{
    public static class Constants
    {
        public static DateTime ThisYear => new DateTime(DateTime.Now.Year, 1, 1);

        public static DateTime ThisSemester => new DateTime(DateTime.Now.Year, (DateTime.Now.Month / 7) * 7 + 1, 1);
    }
}
