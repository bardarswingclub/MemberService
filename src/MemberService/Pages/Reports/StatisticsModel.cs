namespace MemberService.Pages.Reports;

public class StatisticsModel
{
    /// <summary>Semestertitler i kronologisk rekkefølge (x-aksen for kursgrafene).</summary>
    public List<string> Semesters { get; set; } = new();

    /// <summary>Ett punkt per år: antall unike medlemmer med betalt medlemskap.</summary>
    public List<PerYear> Members { get; set; } = new();

    /// <summary>Ett punkt per år: antall unike personer som har betalt kursavgift.</summary>
    public List<PerYear> CoursePayers { get; set; } = new();

    /// <summary>Kurs med sammenlignbart navn på tvers av semestre.</summary>
    public List<CourseSeries> Courses { get; set; } = new();

    public record PerYear(int Year, int Count);

    public class CourseSeries
    {
        public string Name { get; set; }

        /// <summary>Samme lengde som <see cref="StatisticsModel.Semesters"/>. Null = kurset ble ikke holdt.</summary>
        public List<int?> Counts { get; set; } = new();

        public int Total => Counts.Sum(c => c ?? 0);
    }
}
