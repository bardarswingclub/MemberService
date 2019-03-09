using System;
using System.Linq;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                    SignupOpensAt = model.SignupOpensAt,
                    SignupClosesAt = model.SignupClosesAt,
                    AllowPartnerSignup = model.AllowPartnerSignup,
                    RoleSignup = model.RoleSignup
                }
            };

            await _database.AddAsync(entity);
            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(View), new { id = entity.Id });
        }

        public async Task<IActionResult> View(Guid id)
        {
            var eventEntry = await _database.Events
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == id);

            if (eventEntry == null)
            {
                return NotFound();
            }

            return View(eventEntry);
        }

        private async Task<MemberUser> GetCurrentUser()
            => await _database.Users
                .SingleUser(_userManager.GetUserId(User));
    }
}