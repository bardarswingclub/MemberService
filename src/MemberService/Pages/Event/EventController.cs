using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Clave.Expressionify;
using Clave.ExtensionMethods;
using MemberService.Data;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;

namespace MemberService.Pages.Event
{
    [Authorize(Roles = Roles.COORDINATOR_OR_ADMIN)]
    public class EventController : Controller
    {
        private readonly MemberContext _database;
        private readonly UserManager<MemberUser> _userManager;
        private readonly IEmailSender _emailSender;

        public EventController(
            MemberContext database,
            UserManager<MemberUser> userManager,
            IEmailSender emailSender)
        {
            _database = database;
            _userManager = userManager;
            _emailSender = emailSender;
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
                    AllowPartnerSignup = model.AllowPartnerSignup,
                    RoleSignup = model.RoleSignup,
                    SignupHelp = model.SignupHelp
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

            return View(new EventModel
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Options = model.SignupOptions,
                Signups = statuses.Select(s => EventSignupStatusModel.Create(s, model.Signups.Where(x => x.Status == s))).ToReadOnlyCollection(),
                Archived = model.Archived
            });
        }

        [HttpPost]
        public async Task<IActionResult> View(Guid id, [FromForm] EventSaveModel input)
        {
            var selected = input.Leads
                .Concat(input.Follows)
                .Concat(input.Solos)
                .Where(l => l.Selected)
                .Select(l => l.Id);

            if (input.Status != Status.Unknown)
            {
                var eventEntry = await _database.Events
                    .Include(e => e.Signups)
                        .ThenInclude(s => s.User)
                    .SingleOrDefaultAsync(e => e.Id == id);

                foreach (var signup in selected)
                {
                    var eventSignup = eventEntry.Signups.Single(s => s.Id == signup);
                    eventSignup.Status = input.Status;

                    if (input.SendEmail)
                    {
                        try
                        {
                            await _emailSender.SendEmailAsync(
                                eventSignup.User.Email,
                                GetSubject(input.Status, eventEntry.Title),
                                GetMessage(input.Status, eventSignup));
                        }
                        catch
                        {
                            // Mail sending might fail, but that should't stop us
                        }
                    }
                }

                await _database.SaveChangesAsync();
            }

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
                RoleSignup = model.SignupOptions.RoleSignup,
                AllowPartnerSignup = model.SignupOptions.AllowPartnerSignup,
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
                SignupHelp = model.SignupOptions.SignupHelp
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
            entity.SignupOptions.AllowPartnerSignup = model.AllowPartnerSignup;
            entity.SignupOptions.RoleSignup = model.RoleSignup;
            entity.SignupOptions.SignupHelp = model.SignupHelp;

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
            else if(status == "archive")
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

        private static string GetSubject(Status status, string title)
        {
            switch (status)
            {
                case Status.Approved:
                    return $"Du har fått plass på {title}";
                case Status.WaitingList:
                    return $"Du er på ventelisten til {title}";
                case Status.Denied:
                    return $"Du har mistet plassen din til {title}";
                default:
                    throw new Exception($"Unknown status {status}");
            }
        }

        private string GetMessage(Status status, EventSignup model)
        {
            switch (status)
            {
                case Status.Approved:
                    return $@"<h2>Hei {model.User.FullName}</h2>

                        <p>
                            Du har fått plass på {model.Event.Title}!
                        </p>

                        <p>
                            Du må trykke <a href='{Url.Action("Signup", "Signup", new { id = model.EventId })}'>her for å bekrefte at du ønsker plassen</a>.
                            Hvis du ikke gjør det kan plassen din bli gitt til noen andre.
                        </p>

                        <i>Hilsen</i><br>
                        <i>Bårdar Swing Club</i>";
                case Status.WaitingList:
                    return $@"<h2>Hei {model.User.FullName}</h2>

                        <p>
                            Du har på ventelisten til {model.Event.Title}.
                        </p>

                        <p>
                            Det er mange som ønsker å delta på {model.Event.Title} og akkurat nå er det ikke plass til alle, så du er på ventelisten.
                            Du vil få beskjed om det blir ledig plass til deg eller om det blir fullt. <a href='{Url.Action("Signup", "Signup", new { id = model.EventId })}'>Her kan du se påmeldingsstatusen din</a>.
                        </p>

                        <i>Hilsen</i><br>
                        <i>Bårdar Swing Club</i>";
                case Status.Denied:
                    return $@"<h2>Hei {model.User.FullName}</h2>

                        <p>
                            Du har ikke lenger plass på {model.Event.Title}.
                        </p>

                        <p>
                            Du har mistet plassen din til arrangementet. Hvis du lurer på hvorfor kan du svar på denne mailen.
                        </p>

                        <i>Hilsen</i><br>
                        <i>Bårdar Swing Club</i>";
                default:
                    throw new Exception($"Unknown status {status}");
            }
        }
    }
}
