namespace MemberService.Pages.Reports;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                .Where(g => g.Count() >= 2)
                .ToList();

            if (!perUser.Any()) continue;

            var block = new MultiClassModel.SemesterBlock { Title = semester.Title };

            foreach (var countGroup in perUser.GroupBy(g => g.Count()).OrderByDescending(g => g.Key))
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
                            case DanceStyle.LindyHop:   row.LindyHopOnly++;   break;
                            case DanceStyle.SlowBalboa: row.SlowBalobaOnly++; break;
                            case DanceStyle.Balboa:     row.BalobaOnly++;     break;
                            case DanceStyle.Shag:       row.ShagOnly++;       break;
                            case DanceStyle.SoloJazz:   row.SoloJazzOnly++;   break;
                            default:                    row.UnknownOnly++;    break;
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
                                case DanceStyle.SlowBalboa: row.LHAndSlowBalboa++; break;
                                case DanceStyle.Balboa:     row.LHAndBalboa++;     break;
                                case DanceStyle.Shag:       row.LHAndShag++;       break;
                                case DanceStyle.SoloJazz:   row.LHAndSoloJazz++;   break;
                                default:                    row.OtherMixed++;      break;
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
}
