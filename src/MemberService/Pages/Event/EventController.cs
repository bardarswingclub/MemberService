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

        public async Task<IActionResult> Index(bool archived = false)
        {
            var events = await _database.Events
                .Include(e => e.SignupOptions)
                .Include(e => e.Signups)
                .AsNoTracking()
                .Where(e => archived || e.Archived == false)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(events);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new EventInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] EventInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = new Data.Event
            {
                Title = model.Title,
                Description = model.Description,
                CreatedAt = DateTime.UtcNow,
                CreatedByUser = await GetCurrentUser(),
                SignupOptions = new EventSignupOptions
                {
                    RequiresMembershipFee = model.RequiresMembershipFee,
                    RequiresTrainingFee = model.RequiresTrainingFee,
                    RequiresClassesFee = model.RequiresClassesFee,
                    PriceForMembers = model.PriceForMembers,
                    PriceForNonMembers = model.PriceForNonMembers,
                    SignupOpensAt = GetUtc(model.EnableSignupOpensAt, model.SignupOpensAtDate, model.SignupOpensAtTime),
                    SignupClosesAt = GetUtc(model.EnableSignupClosesAt, model.SignupClosesAtDate, model.SignupClosesAtTime),
                    SignupHelp = model.SignupHelp,
                    RoleSignup = model.RoleSignup,
                    RoleSignupHelp = model.RoleSignupHelp,
                    AllowPartnerSignup = model.AllowPartnerSignup,
                    AllowPartnerSignupHelp = model.AllowPartnerSignupHelp,
                    AutoAcceptedSignups = model.AutoAcceptedSignups
                }
            };

            await _database.AddAsync(entity);
            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(View), new { id = entity.Id });
        }

        [HttpGet]
        public async Task<IActionResult> View(Guid id)
        {
            var model = await _database.Events
                .Include(e => e.SignupOptions)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.User)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.AuditLog)
                        .ThenInclude(l => l.User)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.Partner)
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            var statuses = new[] {
                Status.AcceptedAndPayed,
                Status.Approved,
                Status.WaitingList,
                Status.Recommended,
                Status.Pending,
                Status.RejectedOrNotPayed,
                Status.Denied,
            };

            foreach (var (partner, signup) in model.Signups
                .Select(s => s.Partner)
                .WhereNotNull()
                .Join(model.Signups, p => p.Id, s => s.UserId))
            {
                partner.EventSignups.Add(signup);
            }

            return View(new EventModel
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Options = model.SignupOptions,
                Signups = statuses
                    .Select(s => (s, model.Signups.Where(x => x.Status == s)))
                    .Select(EventSignupStatusModel.Create)
                    .ToReadOnlyCollection(),
                Archived = model.Archived
            });
        }

        [HttpPost]
        public async Task<IActionResult> View(Guid id, [FromForm] EventSaveModel input)
        {
            var currentUser = await GetCurrentUser();

            var selected = input.Leads
                .Concat(input.Follows)
                .Concat(input.Solos)
                .Where(l => l.Selected)
                .Select(l => l.Id);

            var eventEntry = await _database.Events
                .Include(e => e.Signups)
                    .ThenInclude(s => s.User)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.AuditLog)
                .SingleOrDefaultAsync(e => e.Id == id);

            foreach (var signup in selected)
            {
                var eventSignup = eventEntry.Signups.Single(s => s.Id == signup);

                bool statusChanged = input.Status != Status.Unknown;
                if (statusChanged)
                {
                    eventSignup.Status = input.Status;
                }

                if (input.SendEmail)
                {
                    try
                    {
                        if (input.SendCustomEmail)
                        {
                            await _emailService.SendCustomEmail(
                                eventSignup.User.Email,
                                input.Subject,
                                input.Message,
                                new EventStatusModel
                                {
                                    Name = eventSignup.User.FullName,
                                    Title = eventEntry.Title,
                                    Link = await SignupLink(eventSignup.User, eventEntry)
                                });

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
                                new EventStatusModel
                                {
                                    Name = eventSignup.User.FullName,
                                    Title = eventEntry.Title,
                                    Link = await SignupLink(eventSignup.User, eventEntry)
                                });

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
                else if (statusChanged)
                {
                    eventSignup.AuditLog.Add($"Moved to {input.Status} ", currentUser);
                }
            }

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(View), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var model = await _database.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            var (signupOpensAtDate, signupOpensAtTime) = GetLocal(model.SignupOptions.SignupOpensAt);
            var (signupClosesAtDate, signupClosesAtTime) = GetLocal(model.SignupOptions.SignupClosesAt);

            return View(new EventInputModel
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                EnableSignupOpensAt = model.SignupOptions.SignupOpensAt.HasValue,
                SignupOpensAtDate = signupOpensAtDate,
                SignupOpensAtTime = signupOpensAtTime,
                EnableSignupClosesAt = model.SignupOptions.SignupClosesAt.HasValue,
                SignupClosesAtDate = signupClosesAtDate,
                SignupClosesAtTime = signupClosesAtTime,
                PriceForMembers = model.SignupOptions.PriceForMembers,
                PriceForNonMembers = model.SignupOptions.PriceForNonMembers,
                RequiresMembershipFee = model.SignupOptions.RequiresMembershipFee,
                RequiresTrainingFee = model.SignupOptions.RequiresTrainingFee,
                RequiresClassesFee = model.SignupOptions.RequiresClassesFee,
                SignupHelp = model.SignupOptions.SignupHelp,
                RoleSignup = model.SignupOptions.RoleSignup,
                RoleSignupHelp = model.SignupOptions.RoleSignupHelp,
                AllowPartnerSignup = model.SignupOptions.AllowPartnerSignup,
                AllowPartnerSignupHelp = model.SignupOptions.AllowPartnerSignupHelp,
                AutoAcceptedSignups = model.SignupOptions.AutoAcceptedSignups
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [FromForm] EventInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = await _database.Events
                .Include(e => e.SignupOptions)
                .SingleOrDefaultAsync(e => e.Id == id);

            if (entity == null)
            {
                return NotFound();
            }

            entity.Title = model.Title;
            entity.Description = model.Description;

            entity.SignupOptions.RequiresMembershipFee = model.RequiresMembershipFee;
            entity.SignupOptions.RequiresTrainingFee = model.RequiresTrainingFee;
            entity.SignupOptions.RequiresClassesFee = model.RequiresClassesFee;
            entity.SignupOptions.PriceForMembers = model.PriceForMembers;
            entity.SignupOptions.PriceForNonMembers = model.PriceForNonMembers;
            entity.SignupOptions.SignupOpensAt = GetUtc(model.EnableSignupOpensAt, model.SignupOpensAtDate, model.SignupOpensAtTime);
            entity.SignupOptions.SignupClosesAt = GetUtc(model.EnableSignupClosesAt, model.SignupClosesAtDate, model.SignupClosesAtTime);
            entity.SignupOptions.SignupHelp = model.SignupHelp;
            entity.SignupOptions.RoleSignup = model.RoleSignup;
            entity.SignupOptions.RoleSignupHelp = model.RoleSignupHelp;
            entity.SignupOptions.AllowPartnerSignup = model.AllowPartnerSignup;
            entity.SignupOptions.AllowPartnerSignupHelp = model.AllowPartnerSignupHelp;
            entity.SignupOptions.AutoAcceptedSignups = model.AutoAcceptedSignups;

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(View), new { id = entity.Id });
        }

        [HttpPost]
        public async Task<IActionResult> SetStatus(Guid id, [FromForm] string status)
        {
            var model = await _database.Events
                .Where(e => e.Id == id)
                .Select(e => e.SignupOptions)
                .Include(e => e.Event)
                .SingleOrDefaultAsync();

            if (model == null)
            {
                return NotFound();
            }

            if (status == "open")
            {
                model.SignupOpensAt = DateTime.UtcNow;
                model.SignupClosesAt = null;
            }
            else if (status == "close")
            {
                model.SignupClosesAt = DateTime.UtcNow;
            }
            else if (status == "archive")
            {
                model.SignupClosesAt = model.SignupClosesAt ?? DateTime.UtcNow;
                model.Event.Archived = true;
            }

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(View), new { id });
        }

        private DateTime? GetUtc(bool enable, string date, string time)
        {
            if (!enable) return null;

            var dateTime = $"{date}T{time}:00";

            var localDateTime = LocalDateTimePattern.GeneralIso.Parse(dateTime).GetValueOrThrow();

            return localDateTime.InZoneLeniently(Constants.TimeZoneOslo).ToDateTimeUtc();
        }

        private (string Date, string Time) GetLocal(DateTime? utc)
        {
            if (!utc.HasValue) return (null, null);

            var result = utc.Value.ToOsloZone();

            return (result.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), result.TimeOfDay.ToString("HH:mm", CultureInfo.InvariantCulture));
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
