namespace MemberService.Pages.Event;

using Clave.Expressionify;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Semester;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[Authorize]
public class EventController : Controller
{
    private readonly MemberContext _database;

    private readonly IEmailService _emailService;

    private readonly ILoginService _linker;

    private readonly ILogger<EventController> _logger;

    public EventController(
        MemberContext database,
        IEmailService emailService,
        ILoginService linker,
        ILogger<EventController> logger)
    {
        _database = database;
        _emailService = emailService;
        _linker = linker;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanEditEvent))]
    public async Task<IActionResult> SetStatus(Guid id, [FromForm] string status)
    {
        var ev = await _database.EditEvent(id, e => e.SetEventStatus(status));

        if (ev.Archived)
        {
            if (ev.SemesterId.HasValue)
            {
                return RedirectToAction(nameof(SemesterController.Index), "Semester");
            }
            else
            {
                return RedirectToPage("/Event");
            }
        }

        return RedirectToPage("/Event/View", new { id });
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanEditEventSignup))]
    public async Task<IActionResult> EditSignup(Guid id)
    {
        var signup = await _database.EventSignups
            .Expressionify()
            .Select(e => EditSignupModel.Create(e, _database.Users))
            .FirstOrDefaultAsync(e => e.Id == id);

        return View(signup);
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanEditEventSignup))]
    public async Task<IActionResult> EditSignup(Guid id, [FromForm] DanceRole role, [FromForm] string partnerEmail, [FromForm] Guid? eventId)
    {
        var signup = await _database.EventSignups
            .Include(e => e.AuditLog)
            .FirstOrDefaultAsync(e => e.Id == id);

        var user = await _database.Get(User);

        if (ModelState.IsValid)
        {
            partnerEmail = partnerEmail?.Trim().Normalize().ToUpperInvariant();
            var log = "Admin edited";
            if (signup.Role != role)
            {
                log += $"\n\n{signup.Role} -> {role}";
                signup.Role = role;
            }

            if (signup.PartnerEmail != partnerEmail)
            {
                log += $"\n\n{signup.PartnerEmail} -> {partnerEmail}";
                signup.PartnerEmail = partnerEmail;
            }

            if (eventId.HasValue && signup.EventId != eventId)
            {
                log += $"\n\n{signup.EventId} -> {eventId}";
                signup.EventId = eventId.Value;
            }

            signup.AuditLog.Add(log, user);
            await _database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(View), new { id = eventId ?? signup.EventId });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanCreateEvent))]
    public async Task<IActionResult> Copy(Guid id)
    {
        var user = await _database.Get(User);
        var entry = await _database.CloneEvent(id, user);
        return RedirectToAction(nameof(View), new { id = entry.Id });
    }
}
