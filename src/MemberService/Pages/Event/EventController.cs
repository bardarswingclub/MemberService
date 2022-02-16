namespace MemberService.Pages.Event;

using Clave.Expressionify;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Emails.Event;
using MemberService.Pages.Semester;
using MemberService.Pages.Signup;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[Authorize]
public class EventController : Controller
{
    private readonly MemberContext _database;

    private readonly UserManager<User> _userManager;

    private readonly IEmailService _emailService;

    private readonly ILoginService _linker;

    private readonly ILogger<EventController> _logger;

    public EventController(
        MemberContext database,
        UserManager<User> userManager,
        IEmailService emailService,
        ILoginService linker,
        ILogger<EventController> logger)
    {
        _database = database;
        _userManager = userManager;
        _emailService = emailService;
        _linker = linker;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanListEvents))]
    public async Task<IActionResult> Index(bool archived = false)
    {
        var model = await _database.GetEvents(GetUserId(), archived);

        return View(model);
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanCreateEvent))]
    public IActionResult Create(EventType type)
    {
        return View(new EventInputModel
        {
            Type = type
        });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanCreateEvent))]
    public async Task<IActionResult> Create([FromForm] EventInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = model.ToEntity(await GetCurrentUser());

        if (entity.Type == EventType.Party && User.CanCreateParty())
        {
            _database.Events.Add(entity);
        }
        else if (entity.Type == EventType.Workshop && User.CanCreateWorkshop())
        {
            _database.Events.Add(entity);
        }
        else
        {
            return Forbid();
        }

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(View), new { id = entity.Id });
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanCreateSemesterEvent))]
    public async Task<IActionResult> CreateClass()
    {
        var semester = await _database.Semesters.Current();

        var (date, time) = semester.SignupOpensAt.GetLocalDateAndTime();

        return View(new EventInputModel
        {
            Type = EventType.Class,
            SemesterId = semester.Id,
            SignupOpensAtDate = date,
            SignupOpensAtTime = time,
            EnableSignupOpensAt = true,
        });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanCreateSemesterEvent))]
    public async Task<IActionResult> CreateClass([FromForm] EventInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = model.ToEntity(await GetCurrentUser());

        var semester = await _database.Semesters.Current();

        semester.Courses.Add(entity);

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(View), new { id = entity.Id });
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanViewEvent))]
    public async Task<IActionResult> View(
        Guid id,
        [FromQuery] EventFilterModel filter)
    {
        var model = await _database.GetEventModel(
            id,
            filter?.SignedUpBefore,
            filter?.Priority,
            filter?.Name,
            filter?.ExcludeAcceptedElsewhere ?? false,
            filter?.ExcludeApprovedElsewhere ?? false,
            filter?.ExcludeRecommendedElsewhere ?? false,
            filter?.OnlyDeniedElsewhere ?? false,
            filter?.OnlyRejectedElsewhere ?? false,
            filter?.OnlyWaitingListElsewhere ?? false);

        if (model == null)
        {
            return NotFound();
        }

        model.Filter = filter;

        return View(model);
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanSetEventSignupStatus))]
    public async Task<IActionResult> View(Guid id, [FromForm] EventSaveModel input)
    {
        var currentUser = await GetCurrentUser();

        var selected = input.GetSelected();

        var statusChanged = input.Status != Status.Unknown;

        await _database.EditEvent(id, async eventEntry =>
        {
            foreach (var signup in selected)
            {
                var eventSignup = eventEntry.Signups.Single(s => s.Id == signup);

                if (statusChanged)
                {
                    eventSignup.Status = input.Status;
                }

                if (input.SendEmail)
                {
                    var model = new EventStatusModel(
                        eventEntry.Title,
                        await SignupLink(eventSignup.User, eventEntry));

                    await SendEmail(input, model, currentUser, statusChanged, eventSignup);
                }
                else if (statusChanged)
                {
                    eventSignup.AuditLog.Add($"Moved to {input.Status} ", currentUser);
                }
            }
        });

        return RedirectToAction(nameof(View), new { id });
    }

    private async Task SendEmail(EventSaveModel input, EventStatusModel model, User currentUser, bool statusChanged, EventSignup eventSignup)
    {
        try
        {
            await _emailService.SendCustomEmail(
                eventSignup.User,
                input.Subject,
                input.Message,
                model,
                input.ReplyToMe ? currentUser : null);

            if (statusChanged)
            {
                eventSignup.AuditLog.Add($"Moved to {input.Status} and sent email\n\n---\n\n> {input.Subject}\n\n{input.Message}", currentUser);
            }
            else
            {
                eventSignup.AuditLog.Add($"Sent email\n\n---\n\n> {input.Subject}\n\n{input.Message}", currentUser);
            }
        }
        catch (Exception e)
        {
            // Mail sending might fail, but that should't stop us
            eventSignup.AuditLog.Add($"Tried to send email, but failed with message {e.Message}", currentUser);
            _logger.LogError(e, "Failed to send email");
        }
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanEditEvent))]
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await _database.GetEventInputModel(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanEditEvent))]
    public async Task<IActionResult> Edit(Guid id, [FromForm] EventInputModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _database.EditEvent(id, e => e.UpdateEvent(model));

        return RedirectToAction(nameof(View), new { id });
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
                return RedirectToAction(nameof(Index));
            }
        }

        return RedirectToAction(nameof(View), new { id });
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanEditEventOrganizers))]
    public async Task<IActionResult> EditOrganizers(Guid id)
    {
        var model = await _database.Events
            .Expressionify()
            .Select(e => EditOrganizersModel.Create(e))
            .FirstOrDefaultAsync(e => e.EventId == id);

        return View(model);
    }


    [HttpGet]
    [Authorize(nameof(Policy.CanEditEventOrganizers))]
    public async Task<object> Users(Guid id, string q)
    {
        var model = await _database.Users
            .Expressionify()
            .Except(_database.EventOrganizers
                .Where(o => o.EventId == id)
                .Select(o => o.User))
            .Where(u => u.NameMatches(q))
            .Select(u => new
            {
                value = u.Id,
                text = u.FullName + " (" + u.Email + ")"
            })
            .Take(10)
            .ToListAsync();

        return model;
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanEditEventOrganizers))]
    public async Task<IActionResult> AddOrganizer(Guid id, [FromForm] EditEventOrganizerInput input)
    {
        _database.EventOrganizers.Add(new EventOrganizer
        {
            UserId = input.UserId,
            EventId = id,
            UpdatedAt = DateTime.UtcNow,
            UpdatedByUser = await GetCurrentUser(),
            CanEdit = input.CanEdit,
            CanEditSignup = input.CanEditSignup,
            CanSetSignupStatus = input.CanSetSignupStatus,
            CanEditOrganizers = input.CanEditOrganizers,
            CanSetPresence = input.CanSetPresence,
            CanAddPresenceLesson = input.CanAddPresenceLesson,
        });

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(EditOrganizers), new { id });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanEditEventOrganizers))]
    public async Task<IActionResult> EditOrganizer(Guid id, [FromForm] EditEventOrganizerInput input, [FromForm] bool remove)
    {
        var organizer = await _database.EventOrganizers
            .FindAsync(id, input.UserId);

        if (organizer != null)
        {
            if (remove)
            {
                _database.EventOrganizers.Remove(organizer);
            }
            else
            {
                organizer.UpdatedAt = DateTime.UtcNow;
                organizer.UpdatedByUser = await GetCurrentUser();
                organizer.CanEdit = input.CanEdit;
                organizer.CanEditSignup = input.CanEditSignup;
                organizer.CanSetSignupStatus = input.CanSetSignupStatus;
                organizer.CanEditOrganizers = input.CanEditOrganizers;
                organizer.CanSetPresence = input.CanSetPresence;
                organizer.CanAddPresenceLesson = input.CanAddPresenceLesson;
            }

            await _database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(EditOrganizers), new { id });
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanEditEventOrganizers))]
    public async Task<IActionResult> RemoveOrganizer(Guid id, [FromForm] string userId)
    {
        var organizer = await _database.EventOrganizers
            .FindAsync(id, userId);

        if (organizer != null)
        {
            _database.EventOrganizers.Remove(organizer);

            await _database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(EditOrganizers));
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

        var user = await GetCurrentUser();

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
        var entry = await _database.CloneEvent(id, await GetCurrentUser());
        return RedirectToAction(nameof(View), new { id = entry.Id });
    }

    private async Task<User> GetCurrentUser() => await _database.Users.SingleUser(GetUserId());

    private string GetUserId() => _userManager.GetUserId(User);

    private async Task<string> SignupLink(User user, Data.Event e)
    {
        var targetLink = SignupLink(e.Id, e.Title);

        return await _linker.LoginLink(user, targetLink);
    }

    private string SignupLink(Guid id, string title) => Url.Action(
        nameof(SignupController.Event),
        "Signup",
        new
        {
            id,
            slug = title.Slugify()
        });

    public record EditEventOrganizerInput
    {
        public string UserId { get; init; }

        public bool CanEdit { get; init; }

        public bool CanEditSignup { get; set; }

        public bool CanSetSignupStatus { get; set; }

        public bool CanEditOrganizers { get; set; }

        public bool CanSetPresence { get; set; }

        public bool CanAddPresenceLesson { get; set; }
    }
}
