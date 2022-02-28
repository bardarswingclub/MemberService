namespace MemberService.Pages.Event.Presence;

using Clave.Expressionify;

using MemberService.Auth;
using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
            .Expressionify()
            .FirstOrDefaultAsync(s => s.Id == id);

        return View(new PresenceModel(model));
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

        var user = await _database.Users.SingleUser(User.GetId());

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
            presence.RegisteredAt = TimeProvider.UtcNow;
            presence.RegisteredBy = user;
        }

        await _database.SaveChangesAsync();

        return Ok();
    }
}
