namespace MemberService.Pages.Reports;

public enum RetentionEra
{
    PreCovid,
    Covid,
    PostCovid
}

public class RetentionModel
{
    /// <summary>Semestre med deltakere i kronologisk rekkefølge. Siste kan være pågående.</summary>
    public List<string> Semesters { get; set; } = new();

    /// <summary>Antall ferdige semestre. Pågående semester er med i personenes semestertall, men ikke i frafallstall.</summary>
    public int CompletedSemesters { get; set; }

    public bool OngoingSemester { get; set; }

    /// <summary>Alle personer med minst ett semester.</summary>
    public GroupStats All { get; set; } = new();

    /// <summary>Personer som ikke lenger er aktive (ikke med i siste ferdige eller pågående semester).</summary>
    public GroupStats Ended { get; set; } = new();

    public int ActiveNow { get; set; }

    /// <summary>Antall personer per antall semestre: indeks 0 = ett semester.</summary>
    public List<int> Distribution { get; set; } = new();

    /// <summary>Hvor stor andel av deltakerne i et semester som var med i neste.</summary>
    public List<Transition> Transitions { get; set; } = new();

    /// <summary>Andel av nye deltakere som er med n semestre etter første semester, per periode.</summary>
    public List<EraCurve> Curves { get; set; } = new();

    public List<CohortYear> CohortYears { get; set; } = new();

    public CovidSummary Covid { get; set; }

    public record Transition(string Semester, RetentionEra Era, int Total, int Returned)
    {
        public double? Percent => Total == 0 ? null : 100.0 * Returned / Total;
    }

    public record CurvePoint(int SemestersAfter, int Attended, int Total)
    {
        public double Percent => Total == 0 ? 0 : 100.0 * Attended / Total;
    }

    public record EraCurve(RetentionEra Era, int Cohorts, int People, List<CurvePoint> Points);

    public record CohortYear(int Year, int NewPeople, RetentionPoint After1, RetentionPoint After2, RetentionPoint After4);

    /// <summary>Null hvis ingen kull fra året kan observeres så langt frem i tid.</summary>
    public record RetentionPoint(int Attended, int Total)
    {
        public double Percent => Total == 0 ? 0 : 100.0 * Attended / Total;
    }

    public class GroupStats
    {
        public int People { get; set; }

        public double Mean { get; set; }

        public double Median { get; set; }

        /// <summary>Typetall: det vanligste antallet semestre.</summary>
        public int Mode { get; set; }

        public int ModeCount { get; set; }

        /// <summary>Personer som bare har vært med ett semester.</summary>
        public int OneSemester { get; set; }

        public double OneSemesterPercent => People == 0 ? 0 : 100.0 * OneSemester / People;

        public int MostSemesters { get; set; }

        public int MostSemestersCount { get; set; }
    }

    public class CovidSummary
    {
        /// <summary>Siste semester før covid. Deltakerne her er utgangspunktet.</summary>
        public string BaseSemester { get; set; }

        public int BasePeople { get; set; }

        /// <summary>Semestre i covid-året (2020) som deltakerne følges gjennom.</summary>
        public List<string> CovidYearSemesters { get; set; } = new();

        public int StayedThroughCovidYear { get; set; }

        /// <summary>Var med forrige semester, men ikke i noen av semestrene i covid-året.</summary>
        public int LeftInCovidYear { get; set; }

        public int LeftButReturnedLater { get; set; }

        public int NeverReturned { get; set; }

        public int StillActive { get; set; }

        /// <summary>Hvor mange av utgangsgruppen som var med i hvert påfølgende ferdige semester.</summary>
        public List<Transition> Following { get; set; } = new();
    }
}
