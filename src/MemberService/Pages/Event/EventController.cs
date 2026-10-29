namespace MemberService.Pages.Event;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Semester;
using MemberService.Pages.Signup;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[Authorize]
public class EventController : Controller
{
    private const int MaxOriginLength = 100;

    private const int MaxSpecialNeedsLength = 500;

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
    [ValidateAntiForgeryToken]
    [Authorize(nameof(Policy.CanEditEvent))]
    public async Task<IActionResult> SetStatus(Guid id, [FromForm] string status, [FromForm] string date)
    {
        var ev = await _database.EditEvent(id, e => e.SetEventStatus(status, date));

        if (ev.Archived)
        {
            if (ev.SemesterId.HasValue)
            {
                return RedirectToAction(nameof(SemesterController.Index), "Semester");
            }
            else
            {
                return RedirectToPage("/Event/Index");
            }
        }

        return RedirectToPage("/Event/View", new { id });
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanEditEventSignup))]
    public async Task<IActionResult> EditSignup(Guid id)
    {
        var signup = await _database.EventSignups
            .Select(e => EditSignupModel.Create(e, _database.Users))
            .FirstOrDefaultAsync(e => e.Id == id);

        return View(signup);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(nameof(Policy.CanEditEventSignup))]
    public async Task<IActionResult> EditSignup(Guid id, [FromForm] DanceRole role, [FromForm(Name = "Input.Preference")] RolePreference preference, [FromForm] string partnerEmail, [FromForm] string origin, [FromForm] string specialNeeds, [FromForm] Guid? eventId, [FromServices] IAuthorizationService authorization)
    {
        // Flyttes påmeldingen til et annet arrangement, må man også ha tilgang til det arrangementet
        if (eventId.HasValue && !await authorization.IsAuthorized(User, eventId.Value, Policy.CanEditEventSignup))
        {
            return Forbid();
        }

        var signup = await _database.EventSignups
            .Include(e => e.Event)
            .Include(e => e.AuditLog)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (signup is null)
        {
            return NotFound();
        }

        if (origin?.Length > MaxOriginLength || specialNeeds?.Length > MaxSpecialNeedsLength)
        {
            return BadRequest();
        }

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

            // Uten valgt radio er feltet ikke postet, og da skal deltakerens valg ligge urørt
            if (preference.IsSingleChoice() && signup.RolePreferences != preference)
            {
                log += $"\n\nChoices: {signup.RolePreferences} -> {preference}";
                signup.RolePreferences = preference;
            }

            if (signup.PartnerEmail != partnerEmail)
            {
                log += $"\n\n{signup.PartnerEmail} -> {partnerEmail}";
                signup.PartnerEmail = partnerEmail;
            }

            // Feltene finnes bare i skjemaet for arrangementer som samler deltakerinfo. Uten denne sjekken
            // ville redigering av andre arrangementer (eller flytting) slettet det deltakeren har fylt ut.
            // Verdiene logges ikke, siden allergier er helseopplysninger.
            if (signup.Event.Type.CollectsParticipantInfo())
            {
                origin = origin.TrimToNull();
                if (signup.Origin != origin)
                {
                    log += "\n\nOrigin changed";
                    signup.Origin = origin;
                }

                specialNeeds = specialNeeds.TrimToNull();
                if (signup.SpecialNeeds != specialNeeds)
                {
                    log += "\n\nSpecial needs changed";
                    signup.SpecialNeeds = specialNeeds;
                }
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
    [ValidateAntiForgeryToken]
    [Authorize(nameof(Policy.CanCreateEvent))]
    public async Task<IActionResult> Copy(Guid id)
    {
        var user = await _database.Get(User);
        var entry = await _database.CloneEvent(id, user);
        return RedirectToAction(nameof(View), new { id = entry.Id });
    }
}
