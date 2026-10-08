namespace MemberService.Pages.Kursplanlegging;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.CoursePlanning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/kursplanlegging")]
[Authorize(nameof(Policy.CanPlanCourses))]
public class CoursePlanApiController(MemberContext database) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<PlanDto>> Get(Guid id)
    {
        var plan = await database.CoursePlans
            .Include(p => p.Courses)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (plan is null) return NotFound();

        return new PlanDto(
            plan.Id,
            plan.Title,
            plan.StartDate,
            plan.EndDate,
            RoomsJson.Parse(plan.RoomsJson),
            HolidaysJson.Parse(plan.HolidaysJson),
            plan.Courses
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Title)
                .Select(CourseDto.Create)
                .ToList());
    }

    [HttpPut("{id}/rooms")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRooms(Guid id, [FromBody] List<Room> rooms)
    {
        if (Limits.Validate(rooms ?? new()) is string error) return BadRequest(error);

        var plan = await database.CoursePlans.FindAsync(id);
        if (plan is null) return NotFound();

        plan.RoomsJson = RoomsJson.Serialize(CoursePlanLogic.Sanitize(rooms ?? new()));
        plan.UpdatedAt = TimeProvider.UtcNow;
        plan.UpdatedBy = User.GetId();

        await database.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/courses")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<CourseDto>> CreateCourse(Guid id, [FromBody] CourseDto input)
    {
        if (input.Validate() is string error) return BadRequest(error);

        var plan = await database.CoursePlans.FindAsync(id);
        if (plan is null) return NotFound();

        var course = new PlannedCourse { CoursePlanId = id };
        input.ApplyTo(course);
        database.PlannedCourses.Add(course);
        plan.UpdatedAt = TimeProvider.UtcNow;
        plan.UpdatedBy = User.GetId();

        await database.SaveChangesAsync();
        return CourseDto.Create(course);
    }

    [HttpPut("courses/{courseId}")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<CourseDto>> UpdateCourse(Guid courseId, [FromBody] CourseDto input)
    {
        if (input.Validate() is string error) return BadRequest(error);

        var course = await database.PlannedCourses.Include(c => c.CoursePlan).FirstOrDefaultAsync(c => c.Id == courseId);
        if (course is null) return NotFound();

        input.ApplyTo(course);
        course.CoursePlan.UpdatedAt = TimeProvider.UtcNow;
        course.CoursePlan.UpdatedBy = User.GetId();

        await database.SaveChangesAsync();
        return CourseDto.Create(course);
    }

    [HttpDelete("courses/{courseId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(Guid courseId)
    {
        var course = await database.PlannedCourses.FindAsync(courseId);
        if (course is null) return NotFound();

        database.PlannedCourses.Remove(course);
        await database.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("parse-booking")]
    [ValidateAntiForgeryToken]
    public ActionResult<CoursePlanLogic.ParsedBooking> ParseBooking([FromBody] ParseBookingRequest request)
    {
        if ((request?.Text?.Length ?? 0) > Limits.BookingText) return BadRequest($"Teksten kan ikke være lengre enn {Limits.BookingText} tegn");

        return CoursePlanLogic.ParseBooking(request?.Text);
    }
}

public record ParseBookingRequest(string Text);

/// <summary>Øvre grenser for det klienten kan lagre, så planen ikke kan vokse ubegrenset.</summary>
public static class Limits
{
    public const int Rooms = 50;
    public const int SlotsPerRoom = 30;
    public const int Dates = 60;
    public const int Name = 100;
    public const int Title = 200;
    public const int Description = 500;
    public const int SignupHelp = 4000;
    public const int Note = 2000;
    public const int BookingText = 20_000;
    public const int Holidays = 50;

    public static string Validate(List<Room> rooms)
    {
        if (rooms.Count > Rooms) return $"Planen kan ha maks {Rooms} saler";

        foreach (var room in rooms.Where(r => r is not null))
        {
            if ((room.Name?.Length ?? 0) > Name || (room.Venue?.Length ?? 0) > Name) return $"Navn og sted på salen kan være maks {Name} tegn";
            if ((room.Slots?.Count ?? 0) > SlotsPerRoom) return $"En sal kan ha maks {SlotsPerRoom} tidspunkter";
            if (room.Slots?.Any(s => (s?.Dates?.Count ?? 0) > Dates) == true) return $"Et tidspunkt kan ha maks {Dates} datoer";
        }

        return null;
    }
}

public record PlanDto(Guid Id, string Title, DateOnly StartDate, DateOnly EndDate, List<Room> Rooms, List<Holiday> Holidays, List<CourseDto> Courses);

public record CourseDto
{
    public Guid Id { get; init; }

    public string Title { get; init; }

    public string Note { get; init; }

    public string Description { get; init; }

    public string SignupHelp { get; init; }

    public int Weeks { get; init; }

    public string Color { get; init; }

    public int SortOrder { get; init; }

    public string RoomId { get; init; }

    public string SlotId { get; init; }

    public string StartTime { get; init; }

    public string EndTime { get; init; }

    public List<DateOnly> Dates { get; init; } = new();

    public Guid? EventId { get; init; }

    public string Validate()
    {
        if (string.IsNullOrWhiteSpace(Title)) return "Kurset må ha et navn";
        if (Title.Trim().Length > Limits.Title) return $"Navnet kan være maks {Limits.Title} tegn";
        if ((Description?.Length ?? 0) > Limits.Description) return $"Beskrivelsen kan være maks {Limits.Description} tegn";
        if ((SignupHelp?.Length ?? 0) > Limits.SignupHelp) return $"Hjelpeteksten kan være maks {Limits.SignupHelp} tegn";
        if ((Note?.Length ?? 0) > Limits.Note) return $"Notatet kan være maks {Limits.Note} tegn";
        if ((Dates?.Count ?? 0) > Limits.Dates) return $"Kurset kan ha maks {Limits.Dates} datoer";
        return null;
    }

    public static CourseDto Create(PlannedCourse c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Note = c.Note,
        Description = c.Description,
        SignupHelp = c.SignupHelp,
        Weeks = c.Weeks,
        Color = c.Color,
        SortOrder = c.SortOrder,
        RoomId = c.RoomId,
        SlotId = c.SlotId,
        StartTime = c.StartTime,
        EndTime = c.EndTime,
        Dates = c.Dates,
        EventId = c.EventId,
    };

    public void ApplyTo(PlannedCourse c)
    {
        var placed = !string.IsNullOrEmpty(RoomId)
            && !string.IsNullOrEmpty(SlotId)
            && CoursePlanLogic.SafeTime(StartTime) is not null
            && CoursePlanLogic.SafeTime(EndTime) is not null;

        c.Title = Title.Trim();
        c.Note = Note;
        c.Description = CoursePlanLogic.SingleLine(Description);
        c.SignupHelp = string.IsNullOrWhiteSpace(SignupHelp) ? null : SignupHelp.Trim();
        c.Weeks = Math.Clamp(Weeks, 1, 52);
        c.Color = CoursePlanLogic.SafeColor(Color);
        c.SortOrder = SortOrder;
        c.RoomId = placed ? RoomId : null;
        c.SlotId = placed ? SlotId : null;
        c.StartTime = placed ? StartTime : null;
        c.EndTime = placed ? EndTime : null;
        c.Dates = placed ? (Dates ?? new()).Distinct().OrderBy(d => d).ToList() : new();
    }
}
