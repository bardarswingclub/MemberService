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
        if (string.IsNullOrWhiteSpace(input.Title)) return BadRequest("Kurset må ha et navn");

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
        if (string.IsNullOrWhiteSpace(input.Title)) return BadRequest("Kurset må ha et navn");

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
        => CoursePlanLogic.ParseBooking(request.Text);
}

public record ParseBookingRequest(string Text);

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
