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

        public async Task<IActionResult> Index()
        {
            var events = await _database.Events
                .Include(e => e.Signups)
                .AsNoTracking()
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();

            return View(events);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateEventModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var entity = new MemberService.Data.Event
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
                .AsNoTracking()
                .Expressionify()
                .Select(e => new EventModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Options = e.SignupOptions,
                    Leads = e.GetSignups(DanceRole.Lead),
                    Follows = e.GetSignups(DanceRole.Follow),
                    Solos = e.GetSignups(DanceRole.None)
                })
                .SingleOrDefaultAsync(e => e.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
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

        [HttpPost]
        public async Task<IActionResult> SetStatus(Guid id, [FromForm] string status)
        {
            var model = await _database.Events
                .Where(e => e.Id == id)
                .Select(e => e.SignupOptions)
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

        private async Task<MemberUser> GetCurrentUser()
            => await _database.Users
                .SingleUser(_userManager.GetUserId(User));
    }
}