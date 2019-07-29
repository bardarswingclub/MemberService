using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Clave.ExtensionMethods;
using MemberService.Data;
using MemberService.Emails.Event;
using MemberService.Pages.Signup;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime.Text;

namespace MemberService.Pages.Event
{
    [Authorize(Roles = Roles.COORDINATOR_OR_ADMIN)]
    public class EventController : Controller
    {
        private readonly MemberContext _database;

        private readonly UserManager<MemberUser> _userManager;

        private readonly IEmailService _emailService;

        private readonly ILoginService _linker;

        private readonly ILogger<EventController> _logger;

        public EventController(
            MemberContext database,
            UserManager<MemberUser> userManager,
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
        public async Task<IActionResult> Index(bool archived = false)
        {
            var model = await _database.GetEvents(archived);

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new EventInputModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] EventInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = model.ToEntity(await GetCurrentUser());

            await _database.AddAsync(entity);
            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(View), new { id = entity.Id });
        }

        [HttpGet]
        public async Task<IActionResult> View(Guid id)
        {
            var model = await _database.GetEventModel(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
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
                            eventSignup.User.FullName,
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

        private async Task SendEmail(EventSaveModel input, EventStatusModel model, MemberUser currentUser, bool statusChanged, EventSignup eventSignup)
        {
            try
            {
                if (input.SendCustomEmail)
                {
                    await _emailService.SendCustomEmail(
                        eventSignup.User.Email,
                        input.Subject,
                        input.Message,
                        model);

                    if (statusChanged)
                    {
                        eventSignup.AuditLog.Add($"Moved to {input.Status} and sent custom email\n\n---\n\n> {input.Subject}\n\n{input.Message}", currentUser);
                    }
                    else
                    {
                        eventSignup.AuditLog.Add($"Sent custom email\n\n---\n\n> {input.Subject}\n\n{input.Message}", currentUser);
                    }
                }
                else if (statusChanged)
                {
                    var sent = await _emailService.SendEventStatusEmail(
                        eventSignup.User.Email,
                        input.Status,
                        model);

                    if (sent)
                    {
                        eventSignup.AuditLog.Add($"Moved to {input.Status} and sent email", currentUser);
                    }
                    else
                    {
                        eventSignup.AuditLog.Add($"Moved to {input.Status} ", currentUser);
                    }
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
            await _database.EditEvent(id, e => e.SetEventStatus(status));

            return RedirectToAction(nameof(View), new { id });
        }

        private async Task<MemberUser> GetCurrentUser()
            => await _database.Users
                .SingleUser(_userManager.GetUserId(User));

        private async Task<string> SignupLink(MemberUser user, Data.Event e)
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
