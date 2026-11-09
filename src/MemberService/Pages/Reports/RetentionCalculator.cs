namespace MemberService.Pages.Reports;

using static RetentionModel;

public static class RetentionCalculator
{
    /// <summary>Første semester regnet som covid (vår 2020: nedstengingen kom midt i semesteret).</summary>
    public static readonly DateTime CovidStart = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Første semester etter covid.</summary>
    public static readonly DateTime CovidEnd = new(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Hvor langt frem i tid kurvene følger nye deltakere.</summary>
    private const int MaxSemestersAfter = 8;

    /// <param name="Title">Semesterets tittel.</param>
    /// <param name="SignupOpensAt">Brukes til å plassere semesteret før, under eller etter covid.</param>
    /// <param name="Ongoing">Semesteret pågår eller står for døren, så det mangler data og regnes ikke som frafall.</param>
    /// <param name="UserIds">Unike personer med plass på minst ett kurs i semesteret.</param>
    public record SemesterInput(string Title, DateTime SignupOpensAt, bool Ongoing, IReadOnlyCollection<string> UserIds);

    public static RetentionEra EraOf(DateTime signupOpensAt)
        => signupOpensAt < CovidStart ? RetentionEra.PreCovid
            : signupOpensAt < CovidEnd ? RetentionEra.Covid
            : RetentionEra.PostCovid;

    /// <param name="semesters">Kronologisk, bare semestre med deltakere. Bare det siste kan være pågående.</param>
    public static RetentionModel Calculate(IReadOnlyList<SemesterInput> semesters)
    {
        var model = new RetentionModel
        {
            Semesters = semesters.Select(s => s.Title).ToList(),
            OngoingSemester = semesters.Count > 0 && semesters[^1].Ongoing
        };

        var completed = semesters.Count - (model.OngoingSemester ? 1 : 0);
        model.CompletedSemesters = completed;

        // Hvilke semestre (indekser) hver person var med i
        var attendance = new Dictionary<string, HashSet<int>>();
        for (var i = 0; i < semesters.Count; i++)
        {
            foreach (var user in semesters[i].UserIds)
            {
                if (!attendance.TryGetValue(user, out var set))
                {
                    attendance[user] = set = new HashSet<int>();
                }

                set.Add(i);
            }
        }

        if (completed == 0 || attendance.Count == 0) return model;

        var lastCompleted = completed - 1;
        bool IsActive(HashSet<int> set) => set.Max() >= lastCompleted;

        var all = attendance.Values.ToList();
        model.All = Stats(all);
        model.Ended = Stats(all.Where(s => !IsActive(s)).ToList());
        model.ActiveNow = all.Count(IsActive);

        var counts = all.Select(s => s.Count).ToList();
        model.Distribution = Enumerable.Range(1, counts.Max()).Select(k => counts.Count(c => c == k)).ToList();

        // Fra semester til neste ferdige semester
        for (var i = 0; i < lastCompleted; i++)
        {
            var inSemester = semesters[i].UserIds.Distinct().ToList();
            model.Transitions.Add(new Transition(
                semesters[i].Title,
                EraOf(semesters[i].SignupOpensAt),
                inSemester.Count,
                inSemester.Count(u => attendance[u].Contains(i + 1))));
        }

        // Nye deltakere: første semester er etter det første i datasettet, ellers vet vi ikke om de er nye
        var cohorts = Enumerable.Range(1, Math.Max(0, lastCompleted))
            .Where(i => i <= lastCompleted)
            .Select(i => (Index: i, Users: attendance.Where(a => a.Value.Min() == i).Select(a => a.Value).ToList()))
            .Where(c => c.Users.Count > 0)
            .ToList();

        RetentionPoint Pooled(IEnumerable<(int Index, List<HashSet<int>> Users)> group, int after)
        {
            var observable = group.Where(c => c.Index + after <= lastCompleted).ToList();
            return new RetentionPoint(
                observable.Sum(c => c.Users.Count(u => u.Contains(c.Index + after))),
                observable.Sum(c => c.Users.Count));
        }

        foreach (var era in Enum.GetValues<RetentionEra>())
        {
            var inEra = cohorts.Where(c => EraOf(semesters[c.Index].SignupOpensAt) == era).ToList();
            var points = new List<CurvePoint>();
            for (var k = 1; k <= MaxSemestersAfter; k++)
            {
                var p = Pooled(inEra, k);
                if (p.Total > 0) points.Add(new CurvePoint(k, p.Attended, p.Total));
            }

            model.Curves.Add(new EraCurve(era, inEra.Count, inEra.Sum(c => c.Users.Count), points));
        }

        model.CohortYears = cohorts
            .GroupBy(c => semesters[c.Index].SignupOpensAt.Year)
            .OrderBy(g => g.Key)
            .Select(g => new CohortYear(g.Key, g.Sum(c => c.Users.Count), Pooled(g, 1), Pooled(g, 2), Pooled(g, 4)))
            .ToList();

        model.Covid = Covid(semesters, attendance, lastCompleted);

        return model;
    }

    private static CovidSummary Covid(
        IReadOnlyList<SemesterInput> semesters,
        Dictionary<string, HashSet<int>> attendance,
        int lastCompleted)
    {
        var baseIndex = -1;
        for (var i = 0; i <= lastCompleted; i++)
        {
            if (EraOf(semesters[i].SignupOpensAt) == RetentionEra.PreCovid) baseIndex = i;
        }

        var covidYear = Enumerable.Range(baseIndex + 1, Math.Max(0, lastCompleted - baseIndex))
            .Where(i => semesters[i].SignupOpensAt.Year == CovidStart.Year)
            .ToList();

        // Uten semestre før og i covid-året finnes ikke sammenligningen
        if (baseIndex < 0 || covidYear.Count == 0) return null;

        var baseUsers = attendance.Values.Where(s => s.Contains(baseIndex)).ToList();
        var covidYearEnd = covidYear.Max();
        var stayed = baseUsers.Where(s => covidYear.Any(s.Contains)).ToList();
        var left = baseUsers.Except(stayed).ToList();

        return new CovidSummary
        {
            BaseSemester = semesters[baseIndex].Title,
            BasePeople = baseUsers.Count,
            CovidYearSemesters = covidYear.Select(i => semesters[i].Title).ToList(),
            StayedThroughCovidYear = stayed.Count,
            LeftInCovidYear = left.Count,
            LeftButReturnedLater = left.Count(s => s.Any(i => i > covidYearEnd)),
            NeverReturned = baseUsers.Count(s => s.Max() == baseIndex),
            StillActive = baseUsers.Count(s => s.Max() >= lastCompleted),
            Following = Enumerable.Range(baseIndex + 1, lastCompleted - baseIndex)
                .Select(i => new Transition(
                    semesters[i].Title,
                    EraOf(semesters[i].SignupOpensAt),
                    baseUsers.Count,
                    baseUsers.Count(s => s.Contains(i))))
                .ToList()
        };
    }

    private static GroupStats Stats(IReadOnlyList<HashSet<int>> people)
    {
        if (people.Count == 0) return new GroupStats();

        var counts = people.Select(s => s.Count).OrderBy(c => c).ToList();
        var mid = counts.Count / 2;
        var mode = counts.GroupBy(c => c).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).First();

        return new GroupStats
        {
            People = counts.Count,
            Mean = counts.Average(),
            Median = counts.Count % 2 == 1 ? counts[mid] : (counts[mid - 1] + counts[mid]) / 2.0,
            Mode = mode.Key,
            ModeCount = mode.Count(),
            OneSemester = counts.Count(c => c == 1),
            MostSemesters = counts[^1],
            MostSemestersCount = counts.Count(c => c == counts[^1])
        };
    }
}
