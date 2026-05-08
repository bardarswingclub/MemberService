namespace MemberService.Pages.Reports;

public class MultiClassModel
{
    public List<SemesterBlock> Semesters { get; set; } = new();

    public class SemesterBlock
    {
        public string Title { get; set; }
        public List<ClassCountRow> Rows { get; set; } = new();
    }

    public class ClassCountRow
    {
        public int ClassCount { get; set; }
        public int Total { get; set; }

        // Single style only
        public int LindyHopOnly { get; set; }
        public int BoogieWoogieOnly { get; set; }
        public int SlowBalobaOnly { get; set; }
        public int BalobaOnly { get; set; }
        public int ShagOnly { get; set; }
        public int SoloJazzOnly { get; set; }
        public int UnknownOnly { get; set; }

        // Lindy Hop + one other style
        public int LHAndBoogieWoogie { get; set; }
        public int LHAndSlowBalboa { get; set; }
        public int LHAndBalboa { get; set; }
        public int LHAndShag { get; set; }
        public int LHAndSoloJazz { get; set; }
        public int LHAndMultiple { get; set; }

        // No Lindy Hop, multiple styles
        public int OtherMixed { get; set; }
    }

    public enum DanceStyle
    {
        Unknown,
        LindyHop,
        BoogieWoogie,
        SlowBalboa,
        Balboa,
        Shag,
        SoloJazz,
    }

    public static DanceStyle ClassifyTitle(string title)
    {
        if (title.Contains("Lindy Hop", StringComparison.OrdinalIgnoreCase)
            || System.Text.RegularExpressions.Regex.IsMatch(title, @"\bLH\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return DanceStyle.LindyHop;

        if (title.Contains("Boogie Woogie", StringComparison.OrdinalIgnoreCase)
            || title.Contains("Boogie", StringComparison.OrdinalIgnoreCase))
            return DanceStyle.BoogieWoogie;

        if (title.Contains("Slow Balboa", StringComparison.OrdinalIgnoreCase))
            return DanceStyle.SlowBalboa;

        if (title.Contains("Balboa", StringComparison.OrdinalIgnoreCase))
            return DanceStyle.Balboa;

        if (title.Contains("Shag", StringComparison.OrdinalIgnoreCase))
            return DanceStyle.Shag;

        if (title.Contains("Solo Jazz", StringComparison.OrdinalIgnoreCase))
            return DanceStyle.SoloJazz;

        return DanceStyle.Unknown;
    }
}
