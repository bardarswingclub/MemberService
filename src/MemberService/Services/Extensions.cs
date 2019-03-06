namespace MemberService.Services
{
    public static class Extensions
    {
        public static string FormatMoney(this long amount) => string.Format("kr {0:0},-", amount / 100);
    }
}