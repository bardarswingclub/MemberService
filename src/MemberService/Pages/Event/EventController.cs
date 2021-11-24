using System;
using System.Linq;
using System.Threading.Tasks;

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

namespace MemberService.Pages.Event
{
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
            var model = await _database.GetEvents(archived);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(EventType type = EventType.Class, Guid? semesterId = null)
        {
            if (!User.CanCreateEvent())
            {
                return Forbid();
            }

            if (semesterId.HasValue)
            {
                var semester = await _database.Semesters.FindAsync(semesterId.Value);

                var (date, time) = semester.SignupOpensAt.GetLocalDateAndTime();

                return View(new EventInputModel
                {
                    Type = type,
                    SemesterId = semesterId,
                    SignupOpensAtDate = date,
                    SignupOpensAtTime = time,
                    EnableSignupOpensAt = true,
                });
            }

            return View(new EventInputModel
            {
                Type = type
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] EventInputModel model)
        {
            if (!User.CanCreateEvent())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = model.ToEntity(await GetCurrentUser());

            if (model.SemesterId.HasValue)
            {
                var semester = await _database.Semesters.FindAsync(model.SemesterId.Value);
                semester.Courses.Add(entity);
            }
            else
            {
                _database.Events.Add(entity);
            }

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
        public async Task<IActionResult> View(Guid id, [FromForm] EventSaveModel input)
        {
            if (!User.CanSetSignupStatus())
            {
                return Forbid();
            }

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
        public async Task<IActionResult> Edit(Guid id)
        {
            if (!User.CanEditEvent())
            {
                return Forbid();
            }

            var model = await _database.GetEventInputModel(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [FromForm] EventInputModel model)
        {
            if (!User.CanEditEvent())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _database.EditEvent(id, e => e.UpdateEvent(model));

            return RedirectToAction(nameof(View), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> SetStatus(Guid id, [FromForm] string status)
        {
            if (!User.CanEditEvent())
            {
                return Forbid();
            }

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
        public async Task<IActionResult> EditOrganizers(Guid id)
        {
            if (!User.CanEditEventOrganizers())
            {
                return Forbid();
            }

            var organizers = await _database.EventOrganizers
                .Expressionify()
                .Where(e => e.EventId == id)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddOrganizer(Guid id, [FromForm] string userId, [FromForm] OrganizerType type)
        {
            if (!User.CanEditEventOrganizers())
            {
                return Forbid();
            }

            _database.EventOrganizers.Add(new EventOrganizer
            {
                UserId = userId,
                EventId = id,
                Type = type,
                UpdatedAt = DateTime.UtcNow,
                UpdatedByUser = await GetCurrentUser()
            });

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(EditOrganizers));
        }

        [HttpPost]
        public async Task<IActionResult> EditOrganizer(Guid id, [FromForm] string userId, [FromForm] OrganizerType type)
        {
            if (!User.CanEditEventOrganizers())
            {
                return Forbid();
            }

            var organizer = await _database.EventOrganizers
                .FirstOrDefaultAsync(o => o.UserId == userId && o.EventId == id);

            if (organizer != null)
            {
                organizer.UpdatedAt = DateTime.UtcNow;
                organizer.UpdatedByUser = await GetCurrentUser();
                organizer.Type = type;

                await _database.SaveChangesAsync();
            }

            return RedirectToAction(nameof(EditOrganizers));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveOrganizer(Guid id, [FromForm] string userId)
        {
            if (!User.CanEditEventOrganizers())
            {
                return Forbid();
            }

            var organizer = await _database.EventOrganizers
                .FirstOrDefaultAsync(o => o.UserId == userId && o.EventId == id);

            if (organizer != null)
            {
                _database.EventOrganizers.Remove(organizer);

                await _database.SaveChangesAsync();
            }

            return RedirectToAction(nameof(EditOrganizers));
        }

        [HttpGet]
        public async Task<IActionResult> EditSignup(Guid id)
        {
            if (!User.CanEditSignup())
            {
                return Forbid();
            }

            var signup = await _database.EventSignups
                .Expressionify()
                .Select(e => EditSignupModel.Create(e, _database.Users))
                .FirstOrDefaultAsync(e => e.Id == id);

            return View(signup);
        }

        [HttpPost]
        public async Task<IActionResult> EditSignup(Guid id, [FromForm] DanceRole role, [FromForm] string partnerEmail, [FromForm] Guid? eventId)
        {
            if (!User.CanEditSignup())
            {
                return Forbid();
            }

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
        public async Task<IActionResult> Copy(Guid id)
        {
            if (!User.CanCreateEvent())
            {
                return new ForbidResult();
            }

            var entry = await _database.CloneEvent(id, await GetCurrentUser());
            return RedirectToAction(nameof(View), new { id = entry.Id });
        }

        private async Task<User> GetCurrentUser()
            => await _database.Users.SingleUser(_userManager.GetUserId(User));

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
    }
}
