namespace MemberService.Tests;

using MemberService.Pages.Reports;

using NUnit.Framework;

using Shouldly;

public class CourseNameNormalizationTests
{
    [TestCase("Lindy Hop 1 - Høst 2024", "Lindy Hop 1 vår 2025")]
    [TestCase("Lindy Hop 1 (H24)", "lindy hop 1 V25")]
    [TestCase("Shag 2026", "Shag, Våren 2025")]
    public void SameCourseAcrossSemesters_GetsSameKey(string a, string b)
        => ReportsController.NormalizeCourseName(a).ShouldBe(ReportsController.NormalizeCourseName(b));

    [Test]
    public void DifferentLevels_GetDifferentKeys()
        => ReportsController.NormalizeCourseName("Lindy Hop 1 høst 2024")
            .ShouldNotBe(ReportsController.NormalizeCourseName("Lindy Hop 2 høst 2024"));
}
