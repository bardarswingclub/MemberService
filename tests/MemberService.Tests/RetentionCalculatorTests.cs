namespace MemberService.Tests;

using MemberService.Pages.Reports;

using NUnit.Framework;

using Shouldly;

using static MemberService.Pages.Reports.RetentionCalculator;

public class RetentionCalculatorTests
{
    private static SemesterInput Sem(string title, int year, int month, bool ongoing, params string[] users)
        => new(title, new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc), ongoing, users);

    [Test]
    public void NoSemesters_GivesEmptyModel()
    {
        var model = Calculate(new List<SemesterInput>());

        model.All.People.ShouldBe(0);
        model.Covid.ShouldBeNull();
    }

    [Test]
    public void Stats_CountSemestersPerPerson()
    {
        var model = Calculate(new[]
        {
            Sem("H18", 2018, 8, false, "a", "b", "c"),
            Sem("V19", 2019, 1, false, "a", "b"),
            Sem("H19", 2019, 8, false, "a"),
            Sem("V20", 2020, 1, false, "a")
        });

        model.All.People.ShouldBe(3);
        model.All.Mean.ShouldBe((4 + 2 + 1) / 3.0);
        model.All.Median.ShouldBe(2);
        model.All.Mode.ShouldBe(1);
        model.All.MostSemesters.ShouldBe(4);
        model.All.MostSemestersCount.ShouldBe(1);
        model.All.OneSemester.ShouldBe(1);
        model.Distribution.ShouldBe(new[] { 1, 1, 0, 1 });
        model.ActiveNow.ShouldBe(1);
        model.Ended.People.ShouldBe(2);
    }

    [Test]
    public void Transitions_ShareReturningNextSemester()
    {
        var model = Calculate(new[]
        {
            Sem("H18", 2018, 8, false, "a", "b", "c", "d"),
            Sem("V19", 2019, 1, false, "a", "b"),
            Sem("H19", 2019, 8, false, "a")
        });

        model.Transitions.Count.ShouldBe(2);
        model.Transitions[0].Total.ShouldBe(4);
        model.Transitions[0].Returned.ShouldBe(2);
        model.Transitions[1].Total.ShouldBe(2);
        model.Transitions[1].Returned.ShouldBe(1);
    }

    [Test]
    public void OngoingSemester_CountsAsActiveButNotAsDropout()
    {
        var model = Calculate(new[]
        {
            Sem("H18", 2018, 8, false, "a", "b"),
            Sem("V19", 2019, 1, false, "a"),
            Sem("H19", 2019, 8, true, "a", "c")
        });

        model.CompletedSemesters.ShouldBe(2);
        model.OngoingSemester.ShouldBeTrue();
        // Bare overgangen mellom ferdige semestre måles
        model.Transitions.Count.ShouldBe(1);
        // a og c er aktive nå, b har sluttet
        model.ActiveNow.ShouldBe(2);
        model.Ended.People.ShouldBe(1);
        model.Ended.OneSemester.ShouldBe(1);
    }

    [Test]
    public void Cohorts_SkipFirstSemesterAndOnlyUseObservableDistance()
    {
        var model = Calculate(new[]
        {
            Sem("H18", 2018, 8, false, "old"),
            Sem("V19", 2019, 1, false, "old", "n1", "n2"),
            Sem("H19", 2019, 8, false, "n1"),
            Sem("V20x", 2019, 12, false, "n1")
        });

        var pre = model.Curves.Single(c => c.Era == RetentionEra.PreCovid);

        // "old" er ikke en ny deltaker, siden vi ikke vet når de begynte
        pre.People.ShouldBe(2);
        pre.Points.Single(p => p.SemestersAfter == 1).Attended.ShouldBe(1);
        pre.Points.Single(p => p.SemestersAfter == 1).Total.ShouldBe(2);
        pre.Points.Single(p => p.SemestersAfter == 2).Attended.ShouldBe(1);
        // Tre semestre frem finnes ikke for kullet som startet V19, bare to
        pre.Points.Any(p => p.SemestersAfter == 3).ShouldBeFalse();
    }

    [Test]
    public void Covid_FollowsPeopleFromLastPreCovidSemester()
    {
        var model = Calculate(new[]
        {
            Sem("H19", 2019, 8, false, "stay", "gone", "back", "never"),
            Sem("V20", 2020, 1, false, "stay"),
            Sem("H20", 2020, 8, false, "stay"),
            Sem("H22", 2022, 8, false, "stay", "back")
        });

        var covid = model.Covid;
        covid.ShouldNotBeNull();
        covid.BaseSemester.ShouldBe("H19");
        covid.BasePeople.ShouldBe(4);
        covid.CovidYearSemesters.ShouldBe(new[] { "V20", "H20" });
        covid.StayedThroughCovidYear.ShouldBe(1);
        covid.LeftInCovidYear.ShouldBe(3);
        covid.LeftButReturnedLater.ShouldBe(1);
        covid.NeverReturned.ShouldBe(2);
        covid.StillActive.ShouldBe(2);
        covid.Following.Select(f => f.Returned).ShouldBe(new[] { 1, 1, 2 });
    }

    [Test]
    public void Covid_IsNullWithoutSemestersBeforeAndIn2020()
    {
        var model = Calculate(new[]
        {
            Sem("H22", 2022, 8, false, "a"),
            Sem("V23", 2023, 1, false, "a")
        });

        model.Covid.ShouldBeNull();
    }

    [TestCase(2019, 8, RetentionEra.PreCovid)]
    [TestCase(2020, 1, RetentionEra.Covid)]
    [TestCase(2021, 8, RetentionEra.Covid)]
    [TestCase(2022, 1, RetentionEra.PostCovid)]
    public void EraOf_ClassifiesBySignupDate(int year, int month, RetentionEra expected)
        => EraOf(new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc)).ShouldBe(expected);
}
