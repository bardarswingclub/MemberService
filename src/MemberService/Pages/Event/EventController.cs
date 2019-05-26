using System;
using System.Linq;
using System.Threading.Tasks;
using Clave.Expressionify;
using Clave.ExtensionMethods;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Text;

namespace MemberService.Pages.Event
{
    [Authorize(Roles = Roles.COORDINATOR_OR_ADMIN)]
    public class EventController : Controller
    {
        private MemberContext _database;
        private UserManager<MemberUser> _userManager;

        public EventController(
            MemberContext database,
            UserManager<MemberUser> userManager)
        {
            _database = database;
            _userManager = userManager;
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
                    RoleSignup = model.RoleSignup
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
                    .SingleOrDefaultAsync(e => e.Id == id);

                foreach (var signup in selected)
                {
                    eventEntry.Signups.Single(s => s.Id == signup).Status = input.Status;
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

            var instant = Instant.FromDateTimeUtc(DateTime.SpecifyKind(utc.Value, DateTimeKind.Utc));
            var result = instant.InZone(Constants.TimeZoneOslo);

            return (result.Date.ToString("yyyy-MM-dd", null), result.TimeOfDay.ToString("HH:mm", null));
        }

        private async Task<MemberUser> GetCurrentUser()
            => await _database.Users
                .SingleUser(_userManager.GetUserId(User));
    }
}
