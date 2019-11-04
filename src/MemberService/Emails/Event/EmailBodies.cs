using System;
using System.IO;
using System.Reflection;

namespace MemberService.Emails.Event
{
    public static class EmailBodies
    {
        private static readonly Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        private static readonly Lazy<string> _approved = new Lazy<string>(() => ReadFile("MemberService.Emails.Event.Approved.md"));
        private static readonly Lazy<string> _denied = new Lazy<string>(() => ReadFile("MemberService.Emails.Event.Denied.md"));
        private static readonly Lazy<string> _waitingList = new Lazy<string>(() => ReadFile("MemberService.Emails.Event.WaitingList.md"));
        private static readonly Lazy<string> _default = new Lazy<string>(() => ReadFile("MemberService.Emails.Event.Default.md"));

        public static string Approved => _approved.Value;
        public static string Denied => _denied.Value;
        public static string WaitingList => _waitingList.Value;
        public static string Default => _default.Value;

        private static string ReadFile(string path)
        {
            using (var stream = ExecutingAssembly.GetManifestResourceStream(path))
            using(var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}