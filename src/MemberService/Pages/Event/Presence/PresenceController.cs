namespace MemberService.Pages.Event.Presence;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
[Route("/Event/{id}/Presence/{action}")]
public class PresenceController : Controller
{
    private readonly MemberContext _database;

    public PresenceController(
        MemberContext database)
    {
        _database = database;
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanViewEvent))]
    public async Task<IActionResult> Index(Guid id)
    {
        var model = await _database
            .Events
            .Include(e => e.Signups)
                .ThenInclude(s => s.User)
            .Include(e => e.Signups)
                .ThenInclude(s => s.Payment)
            .Include(e => e.Signups)
                .ThenInclude(s => s.Presence)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return View(new PresenceModel(model));
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanViewEvent))]
    public async Task<IActionResult> Lesson(Guid id, int lesson = 0)
    {
        var model = await _database
            .Events
            .Include(e => e.Signups)
                .ThenInclude(s => s.User)
            .Include(e => e.Signups)
                .ThenInclude(s => s.Payment)
            .Include(e => e.Signups)
                .ThenInclude(s => s.Presence)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (model == null)
        {
            return NotFound();
        }

        if (lesson < 0 || lesson >= model.LessonCount)
        {
            return RedirectToAction(nameof(Index), new { id });
        }

        return View(new LessonModel(new PresenceModel(model), lesson));
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanAddPresenceLesson))]
    public async Task<IActionResult> AddLesson(Guid id)
    {
        var model = await _database
            .Events
            .FirstOrDefaultAsync(s => s.Id == id);

        model.LessonCount++;

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { id });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanSetPresence))]
    public async Task<IActionResult> SetPresence(
        Guid id,
        [FromForm] string userId,
        [FromForm] int lesson,
        [FromForm] bool present)
    {
        var signup = await _database.EventSignups
                .Where(p => p.EventId == id)
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync();

        if (signup == null)
        {
            return NotFound();
        }

        var user = await _database.Get(User);

        var presence = signup.Presence
            .FirstOrDefault(p => p.Lesson == lesson);

        if (presence == null)
        {
            signup.Presence.Add(new Data.Presence
            {
                Lesson = lesson,
                Present = present,
                RegisteredAt = TimeProvider.UtcNow,
                RegisteredBy = user
            });
        }
        else
        {
            presence.Present = present;
            if (present)
            {
                presence.AbsenceReason = null;
                presence.AbsenceNote = null;
            }

            presence.RegisteredAt = TimeProvider.UtcNow;
            presence.RegisteredBy = user;
        }

        await _database.SaveChangesAsync();

        return Ok();
    }

    /// <param name="status">"present", an <see cref="AbsenceReason"/> name, or empty for not registered</param>
    [HttpPost]
    [Authorize(nameof(Policy.CanSetPresence))]
    public async Task<IActionResult> SetAttendance(
        Guid id,
        [FromForm] string userId,
        [FromForm] int lesson,
        [FromForm] string status,
        [FromForm] string note)
    {
        var present = status == "present";
        AbsenceReason? reason = null;

        if (!present && !string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<AbsenceReason>(status, out var parsed) || !Enum.IsDefined(parsed))
            {
                return BadRequest();
            }

            reason = parsed;
        }

        var signup = await _database.EventSignups
                .Include(p => p.Presence)
                .Where(p => p.EventId == id)
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync();

        if (signup == null)
        {
            return NotFound();
        }

        var user = await _database.Get(User);

        var presence = signup.Presence
            .FirstOrDefault(p => p.Lesson == lesson);

        if (presence == null)
        {
            presence = new Data.Presence { Lesson = lesson };
            signup.Presence.Add(presence);
        }

        presence.Present = present;
        presence.AbsenceReason = reason;
        presence.AbsenceNote = reason == null || string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        presence.RegisteredAt = TimeProvider.UtcNow;
        presence.RegisteredBy = user;

        await _database.SaveChangesAsync();

        return Ok();
    }
}
