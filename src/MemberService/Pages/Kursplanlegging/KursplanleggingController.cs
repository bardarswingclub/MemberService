namespace MemberService.Pages.Kursplanlegging;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.CoursePlanning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanPlanCourses))]
public class KursplanleggingController(MemberContext database) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var plans = await database.CoursePlans
            .OrderByDescending(p => p.StartDate)
            .Select(p => new PlanListModel.Entry(p.Id, p.Title, p.StartDate, p.EndDate, p.Courses.Count, p.Courses.Count(c => c.EventId != null)))
            .ToListAsync();

        var today = DateOnly.FromDateTime(TimeProvider.UtcToday);
        var spring = today.Month >= 7;
        var year = spring ? today.Year + 1 : today.Year;

        return View(new PlanListModel
        {
            Plans = plans,
            Input = new CreatePlanInput
            {
                Title = spring ? $"Våren {year}" : $"Høsten {year}",
                StartDate = spring ? new DateOnly(year, 1, 5) : new DateOnly(year, 8, 15),
                EndDate = spring ? new DateOnly(year, 6, 15) : new DateOnly(year, 12, 15),
                CopyFromId = plans.FirstOrDefault()?.Id,
                CopyRooms = true,
                CopyCourses = true,
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreatePlanInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Title) || input.EndDate < input.StartDate)
        {
            TempData["ErrorMessage"] = "Semesteret må ha et navn, og sluttdato må være etter startdato";
            return RedirectToAction(nameof(Index));
        }

        var plan = new CoursePlan
        {
            Title = input.Title.Trim(),
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            UpdatedAt = TimeProvider.UtcNow,
            UpdatedBy = User.GetId(),
        };

        var holidays = CoursePlanLogic.SuggestHolidays(input.StartDate, input.EndDate);
        plan.HolidaysJson = HolidaysJson.Serialize(holidays);

        if (input.CopyFromId is Guid copyFromId && (input.CopyRooms || input.CopyCourses))
        {
            var source = await database.CoursePlans
                .Include(p => p.Courses)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == copyFromId);

            if (source is not null)
            {
                var rooms = CoursePlanLogic.CopyRooms(RoomsJson.Parse(source.RoomsJson), input.StartDate, input.EndDate);
                plan.RoomsJson = RoomsJson.Serialize(rooms);

                if (input.CopyCourses)
                {
                    foreach (var course in source.Courses)
                    {
                        plan.Courses.Add(CoursePlanLogic.CopyCourse(course, input.CopyRooms ? rooms : new(), holidays));
                    }
                }

                if (!input.CopyRooms)
                {
                    plan.RoomsJson = "[]";
                }
            }
        }

        database.CoursePlans.Add(plan);
        await database.SaveChangesAsync();

        return RedirectToAction(nameof(Plan), new { id = plan.Id });
    }

    [HttpGet("{controller}/{action}/{id}")]
    public async Task<IActionResult> Plan(Guid id)
    {
        var plan = await database.CoursePlans.FindAsync(id);
        if (plan is null) return NotFound();

        return View(plan);
    }

    [HttpPost("{controller}/{action}/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [FromForm] CreatePlanInput input)
    {
        var plan = await database.CoursePlans.FindAsync(id);
        if (plan is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(input.Title) && input.EndDate >= input.StartDate)
        {
            plan.Title = input.Title.Trim();
            plan.StartDate = input.StartDate;
            plan.EndDate = input.EndDate;
            plan.UpdatedAt = TimeProvider.UtcNow;
            plan.UpdatedBy = User.GetId();
            await database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Plan), new { id });
    }

    [HttpPost("{controller}/{action}/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var plan = await database.CoursePlans.FindAsync(id);
        if (plan is not null)
        {
            database.CoursePlans.Remove(plan);
            await database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{controller}/{action}/{id}")]
    public async Task<IActionResult> Timeplan(Guid id)
    {
        var plan = await database.CoursePlans
            .Include(p => p.Courses)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null) return NotFound();

        return View(TimeplanModel.Create(plan));
    }

    [HttpGet("{controller}/{action}/{id}")]
    public async Task<IActionResult> Import(Guid id)
    {
        var model = await CreateImportModel(id);
        if (model is null) return NotFound();

        return View(model);
    }

    [HttpPost("{controller}/{action}/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(Guid id, [FromForm] Guid semesterId, [FromForm] List<Guid> courseIds)
    {
        var plan = await database.CoursePlans
            .Include(p => p.Courses)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null) return NotFound();

        var semester = await database.Semesters.FindAsync(semesterId);
        if (semester is null)
        {
            TempData["ErrorMessage"] = "Du må velge et semester å opprette kursene i. Opprett semesteret først.";
            return RedirectToAction(nameof(Import), new { id });
        }

        var user = await database.Get(User);
        var rooms = RoomsJson.Parse(plan.RoomsJson);
        var count = 0;

        foreach (var course in plan.Courses.Where(c => courseIds.Contains(c.Id) && c.EventId == null))
        {
            var room = rooms.FirstOrDefault(r => r.Id == course.RoomId);
            var slot = room?.Slots.FirstOrDefault(s => s.Id == course.SlotId);
            var entity = course.ToEvent(room, slot, semester.Id, user);

            database.Events.Add(entity);
            course.Event = entity;
            count++;
        }

        await database.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Opprettet {count} kurs i {semester.Title}. Påmeldingen er ikke åpnet.";
        return RedirectToAction("Index", "Semester", new { id = semester.Id });
    }

    [HttpGet("{controller}/{action}/{id}")]
    public async Task<IActionResult> Fridager(Guid id, bool forslag = false)
    {
        var plan = await database.CoursePlans
            .Include(p => p.Courses)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null) return NotFound();

        var holidays = HolidaysJson.Parse(plan.HolidaysJson);
        // Forslag vises når planen ikke har fridager ennå, eller når man ber om dem,
        // slik at forslag man har valgt bort ikke dukker opp igjen.
        var suggestions = holidays.Count == 0 || forslag
            ? CoursePlanLogic.SuggestHolidays(plan.StartDate, plan.EndDate)
                .Where(s => !holidays.Any(h => h.From == s.From && h.To == s.To))
                .ToList()
            : new();

        return View(new HolidaysModel
        {
            Plan = plan,
            Holidays = holidays,
            Suggestions = suggestions,
            AffectedCourses = plan.Courses.Count(c => c.Dates.Any(holidays.IsClosed)),
        });
    }

    [HttpPost("{controller}/{action}/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fridager(Guid id, [FromForm] List<HolidayInput> holidays, [FromForm] bool moveCourses)
    {
        var plan = await database.CoursePlans
            .Include(p => p.Courses)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null) return NotFound();

        var valid = (holidays ?? new())
            .Where(h => h.Keep)
            .Select(h => new Holiday
            {
                Name = string.IsNullOrWhiteSpace(h.Name) ? "Fri" : h.Name.Trim(),
                From = h.From.Value,
                To = h.To is DateOnly to && to >= h.From.Value ? to : h.From.Value,
            })
            .ToList();

        plan.HolidaysJson = HolidaysJson.Serialize(valid);
        plan.UpdatedAt = TimeProvider.UtcNow;
        plan.UpdatedBy = User.GetId();

        var moved = 0;
        if (moveCourses)
        {
            var rooms = RoomsJson.Parse(plan.RoomsJson);
            foreach (var course in plan.Courses.Where(c => c.Dates.Any(valid.IsClosed)))
            {
                var slot = rooms.FirstOrDefault(r => r.Id == course.RoomId)?.Slots.FirstOrDefault(s => s.Id == course.SlotId);
                course.Dates = CoursePlanLogic.MoveOffHolidays(course.Dates, slot?.Dates ?? new(), valid);
                moved++;
            }
        }

        await database.SaveChangesAsync();

        TempData["SuccessMessage"] = moved > 0
            ? $"Fridagene er lagret. {moved} kurs ble flyttet til uka etter der de falt på en fridag."
            : "Fridagene er lagret.";

        return RedirectToAction(nameof(Fridager), new { id });
    }

    [HttpGet("{controller}/{action}/{id}")]
    public async Task<IActionResult> HentKurs(Guid id, string source = null)
    {
        var plan = await database.CoursePlans
            .Include(p => p.Courses)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null) return NotFound();

        var semesters = await database.Semesters
            .Where(s => s.Courses.Any(c => !c.Cancelled))
            .OrderByDescending(s => s.SignupOpensAt)
            .Select(s => new CopyCoursesModel.Source($"semester:{s.Id}", $"{s.Title} (kurs i påmeldingen)"))
            .ToListAsync();

        var plans = await database.CoursePlans
            .Where(p => p.Id != id && p.Courses.Any())
            .OrderByDescending(p => p.StartDate)
            .Select(p => new CopyCoursesModel.Source($"plan:{p.Id}", $"{p.Title} (kursplanlegging)"))
            .ToListAsync();

        var sources = semesters.Concat(plans).ToList();
        source = sources.Any(s => s.Key == source) ? source : sources.FirstOrDefault()?.Key;

        var candidates = await GetCandidates(source);
        var existing = plan.Courses.Select(c => c.Title).ToList();
        var newCandidates = CoursePlanLogic.ExceptExisting(candidates, c => c.Title, existing);

        return View(new CopyCoursesModel
        {
            Plan = plan,
            Sources = sources,
            SelectedSource = source,
            Candidates = newCandidates,
            AlreadyInPlan = candidates.Count - newCandidates.Count,
        });
    }

    [HttpPost("{controller}/{action}/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HentKurs(Guid id, [FromForm] string source, [FromForm] List<string> keys)
    {
        var plan = await database.CoursePlans
            .Include(p => p.Courses)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null) return NotFound();

        var candidates = (await GetCandidates(source))
            .Where(c => keys.Contains(c.Key))
            .ToList();

        var toAdd = CoursePlanLogic.ExceptExisting(candidates, c => c.Title, plan.Courses.Select(c => c.Title));
        var sortOrder = plan.Courses.Select(c => c.SortOrder).DefaultIfEmpty(-1).Max() + 1;

        foreach (var candidate in toAdd)
        {
            plan.Courses.Add(new PlannedCourse
            {
                Title = candidate.Title.Trim(),
                Description = candidate.Description,
                SignupHelp = candidate.SignupHelp,
                Weeks = candidate.Weeks,
                Color = CoursePlanLogic.Colors[sortOrder % CoursePlanLogic.Colors.Length],
                SortOrder = sortOrder++,
            });
        }

        plan.UpdatedAt = TimeProvider.UtcNow;
        plan.UpdatedBy = User.GetId();
        await database.SaveChangesAsync();

        TempData["SuccessMessage"] = toAdd.Count == 0
            ? "Ingen nye kurs ble lagt til."
            : $"La til {toAdd.Count} kurs. De ligger under timeplanen og kan dras inn i salene.";

        return RedirectToAction(nameof(Plan), new { id });
    }

    private async Task<List<CopyCoursesModel.Candidate>> GetCandidates(string source)
    {
        var parts = (source ?? "").Split(':');
        if (parts.Length != 2 || !Guid.TryParse(parts[1], out var sourceId)) return new();

        if (parts[0] == "semester")
        {
            var courses = await database.Events
                .Where(e => e.SemesterId == sourceId && !e.Cancelled)
                .OrderBy(e => e.Title)
                .Select(e => new { e.Id, e.Title, e.LessonCount, e.Description, e.Archived, e.SignupOptions.SignupHelp })
                .ToListAsync();

            return courses
                .Select(e => new CopyCoursesModel.Candidate(
                    e.Id.ToString(),
                    e.Title,
                    e.LessonCount > 0 ? e.LessonCount : 12,
                    string.Join(" · ", new[] { e.Archived ? "Arkivert" : null, FirstLine(e.Description) }.Where(s => !string.IsNullOrWhiteSpace(s))),
                    CoursePlanLogic.StripSchedule(e.Description),
                    e.SignupHelp))
                .ToList();
        }

        if (parts[0] == "plan")
        {
            var plan = await database.CoursePlans
                .Include(p => p.Courses)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == sourceId);

            if (plan is null) return new();

            var rooms = RoomsJson.Parse(plan.RoomsJson);
            return plan.Courses
                .OrderBy(c => c.Title)
                .Select(c =>
                {
                    var room = rooms.FirstOrDefault(r => r.Id == c.RoomId);
                    var slot = room?.Slots.FirstOrDefault(s => s.Id == c.SlotId);
                    return new CopyCoursesModel.Candidate(c.Id.ToString(), c.Title, c.Weeks, CoursePlanLogic.Describe(c, room, slot), c.Description, c.SignupHelp);
                })
                .ToList();
        }

        return new();
    }

    private static string FirstLine(string text)
    {
        var line = (text ?? "").Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0);
        return line is { Length: > 120 } ? line[..120] + "…" : line;
    }

    private async Task<ImportModel> CreateImportModel(Guid id)
    {
        var plan = await database.CoursePlans
            .Include(p => p.Courses)
            .ThenInclude(c => c.Event)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null) return null;

        var rooms = RoomsJson.Parse(plan.RoomsJson);

        var semesters = await database.Semesters
            .OrderByDescending(s => s.SignupOpensAt)
            .Select(s => new ImportModel.SemesterOption(s.Id, s.Title, s.IsActive()))
            .ToListAsync();

        return new ImportModel
        {
            Plan = plan,
            Semesters = semesters,
            Courses = plan.Courses
                .OrderBy(c => c.Dates.Count == 0)
                .ThenBy(c => rooms.FirstOrDefault(r => r.Id == c.RoomId)?.Slots.FirstOrDefault(s => s.Id == c.SlotId)?.Day ?? 8)
                .ThenBy(c => c.StartTime)
                .ThenBy(c => c.Title)
                .Select(c =>
                {
                    var room = rooms.FirstOrDefault(r => r.Id == c.RoomId);
                    var slot = room?.Slots.FirstOrDefault(s => s.Id == c.SlotId);
                    var schedule = CoursePlanLogic.Describe(c, room, slot);
                    return new ImportModel.CourseEntry(c, schedule is null ? null : CoursePlanLogic.CombineDescription(schedule, c.Description), !CoursePlanLogic.IsSoloJazz(c.Title));
                })
                .ToList(),
        };
    }
}
