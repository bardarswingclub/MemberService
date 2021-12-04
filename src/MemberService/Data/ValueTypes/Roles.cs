namespace MemberService.Data.ValueTypes
{
    using System.Linq;

    public static class Roles
    {
        public const string ADMIN = "Admin";

        public const string COORDINATOR = "Coordinator";

        public const string INSTRUCTOR = "Instructor";

        public const string FESTKOM = "Festkom";

        public static string[] All { get; } = typeof(Roles)
            .GetFields()
            .Select(p => p.GetValue(null))
            .OfType<string>()
            .ToArray();
    }
}