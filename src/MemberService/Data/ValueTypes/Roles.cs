namespace MemberService.Data.ValueTypes;

public static class Roles
{
    public const string ADMIN = "Admin";

    public const string FESTKOM = "Festkom";

    public const string STYRET = "Styret";

    public const string WORKSHOPADM = "Workshopadm";
    
    public const string RESSURSPERSON = "Ressursperson";

    public static string[] All { get; } = typeof(Roles)
        .GetFields()
        .Select(p => p.GetValue(null))
        .OfType<string>()
        .ToArray();
}
