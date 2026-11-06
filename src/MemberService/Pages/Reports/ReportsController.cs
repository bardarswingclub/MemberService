namespace MemberService.Pages.Reports;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Text.RegularExpressions;

using static MultiClassModel;

[Authorize]
public class ReportsController : Controller
{
    private readonly MemberContext _database;

    public ReportsController(MemberContext database)
    {
        _database = database;
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanViewReports))]
    public async Task<IActionResult> MultiClass()
    {
        var cutoff = DateTime.UtcNow.AddYears(-3);

        var semesters = await _database.Semesters
            .Where(s => s.SignupOpensAt >= cutoff)
            .OrderByDescending(s => s.SignupOpensAt)
            .Select(s => new
            {
                s.Title,
                Signups = s.Courses
                    .Where(e => !e.Archived && !e.Cancelled)
                    .SelectMany(e => e.Signups
                        .Where(sg => sg.Status == Status.Approved || sg.Status == Status.AcceptedAndPayed)
                        .Select(sg => new { sg.UserId, e.Title }))
            })
            .ToListAsync();

        var model = new MultiClassModel();

        foreach (var semester in semesters)
        {
            var perUser = semester.Signups
                .GroupBy(sg => sg.UserId)
                .ToList();

            if (!perUser.Any()) continue;

            var block = new MultiClassModel.SemesterBlock { Title = semester.Title };

            foreach (var countGroup in perUser.GroupBy(g => g.Select(c => ClassifyTitle(c.Title)).Distinct().Count()).OrderByDescending(g => g.Key))
            {
                var row = new ClassCountRow { ClassCount = countGroup.Key };

                foreach (var userCourses in countGroup)
                {
                    var styles = userCourses
                        .Select(c => ClassifyTitle(c.Title))
                        .Distinct()
                        .ToList();

                    row.Total++;

                    if (styles.Count == 1)
                    {
                        switch (styles[0])
                        {
                            case DanceStyle.LindyHop:    row.LindyHopOnly++;    break;
                            case DanceStyle.BoogieWoogie: row.BoogieWoogieOnly++; break;
                            case DanceStyle.SlowBalboa:  row.SlowBalobaOnly++;  break;
                            case DanceStyle.Balboa:      row.BalobaOnly++;      break;
                            case DanceStyle.Shag:        row.ShagOnly++;        break;
                            case DanceStyle.SoloJazz:    row.SoloJazzOnly++;    break;
                            default:                     row.UnknownOnly++;     break;
                        }
                    }
                    else if (styles.Contains(DanceStyle.LindyHop))
                    {
                        var others = styles.Where(s => s != DanceStyle.LindyHop).ToList();
                        if (others.Count > 1)
                        {
                            row.LHAndMultiple++;
                        }
                        else
                        {
                            switch (others[0])
                            {
                                case DanceStyle.BoogieWoogie: row.LHAndBoogieWoogie++; break;
                                case DanceStyle.SlowBalboa:   row.LHAndSlowBalboa++;   break;
                                case DanceStyle.Balboa:       row.LHAndBalboa++;       break;
                                case DanceStyle.Shag:         row.LHAndShag++;         break;
                                case DanceStyle.SoloJazz:     row.LHAndSoloJazz++;     break;
                                default:                      row.OtherMixed++;        break;
                            }
                        }
                    }
                    else
                    {
                        row.OtherMixed++;
                    }
                }

                block.Rows.Add(row);
            }

            model.Semesters.Add(block);
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanViewReports))]
    public async Task<IActionResult> Statistics()
    {
        var model = new StatisticsModel();

        // Medlemmer og kursavgift per år: unike brukere med ikke-refundert betaling
        var payments = await _database.Payments
            .Where(p => (p.IncludesMembership || p.IncludesClasses) && !p.Refunded)
            .Select(p => new { UserId = p.User.Id, p.PayedAtUtc, p.IncludesMembership, p.IncludesClasses })
            .ToListAsync();

        static List<StatisticsModel.PerYear> CountPerYear<T>(IEnumerable<T> items, Func<T, DateTime> date, Func<T, string> user)
            => items
                .GroupBy(p => date(p).ToOsloZone().Year)
                .OrderBy(g => g.Key)
                .Select(g => new StatisticsModel.PerYear(g.Key, g.Select(user).Distinct().Count()))
                .ToList();

        model.Members = CountPerYear(payments.Where(p => p.IncludesMembership), p => p.PayedAtUtc, p => p.UserId);
        model.CoursePayers = CountPerYear(payments.Where(p => p.IncludesClasses), p => p.PayedAtUtc, p => p.UserId);

        // Deltakere per kurs per semester
        var semesters = await _database.Semesters
            .OrderBy(s => s.SignupOpensAt)
            .Select(s => new
            {
                s.Title,
                Signups = s.Courses
                    .Where(e => !e.Archived && !e.Cancelled)
                    .SelectMany(e => e.Signups
                        .Where(sg => sg.Status == Status.Approved || sg.Status == Status.AcceptedAndPayed)
                        .Select(sg => new { sg.UserId, e.Title }))
            })
            .ToListAsync();

        var series = new Dictionary<string, StatisticsModel.CourseSeries>();

        for (var i = 0; i < semesters.Count; i++)
        {
            var semester = semesters[i];
            var perCourse = semester.Signups
                .GroupBy(sg => NormalizeCourseName(sg.Title))
                .Where(g => g.Key.Length > 0)
                .ToList();

            if (!perCourse.Any()) continue;

            model.Semesters.Add(semester.Title);

            foreach (var course in perCourse)
            {
                if (!series.TryGetValue(course.Key, out var s))
                {
                    s = new StatisticsModel.CourseSeries { Name = DisplayName(course.First().Title) };
                    series[course.Key] = s;
                }

                // Pad opp til inneværende semester, og la nyeste tittel gi visningsnavn
                while (s.Counts.Count < model.Semesters.Count - 1) s.Counts.Add(null);
                s.Name = DisplayName(course.First().Title);
                s.Counts.Add(course.Select(c => c.UserId).Distinct().Count());
            }
        }

        foreach (var s in series.Values)
        {
            while (s.Counts.Count < model.Semesters.Count) s.Counts.Add(null);
        }

        model.Courses = series.Values.OrderBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase).ToList();

        return View(model);
    }

    private static readonly Regex NoiseTokens = new(
        @"\b(19|20)\d{2}\b|\b[hv]\d{2}\b|\b(høst(en)?|vår(en)?|semester|kurs)\b|[()\[\],:;–—-]",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Nøkkel som gjør at "Lindy Hop 1 - Høst 2024" og "Lindy Hop 1 vår 2025" havner i samme serie.</summary>
    public static string NormalizeCourseName(string title)
        => Regex.Replace(NoiseTokens.Replace(title ?? "", " "), @"\s+", " ").Trim().ToLowerInvariant();

    private static string DisplayName(string title)
        => Regex.Replace(NoiseTokens.Replace(title ?? "", " "), @"\s+", " ").Trim();
}
