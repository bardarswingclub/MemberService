namespace MemberService.Emails.Event;

using System.Reflection;

public static class EmailBodies
{
    private static readonly Lazy<string> _approved = new(() => ReadFile("MemberService.Emails.Event.Approved.md"));
    private static readonly Lazy<string> _denied = new(() => ReadFile("MemberService.Emails.Event.Denied.md"));
    private static readonly Lazy<string> _waitingList = new(() => ReadFile("MemberService.Emails.Event.WaitingList.md"));
    private static readonly Lazy<string> _default = new(() => ReadFile("MemberService.Emails.Event.Default.md"));

    public static string Approved => _approved.Value;
    public static string Denied => _denied.Value;
    public static string WaitingList => _waitingList.Value;
    public static string Default => _default.Value;

    private static string ReadFile(string path)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
