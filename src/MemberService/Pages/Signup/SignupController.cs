using System;
using System.Linq;
using System.Threading.Tasks;
using Clave.Expressionify;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Signup
{
    [Authorize]
    public class SignupController : Controller
    {
        private MemberContext _database;
        private UserManager<MemberUser> _userManager;

        public SignupController(
            MemberContext database,
            UserManager<MemberUser> userManager)
        {
            _database = database;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(Guid id)
        {
            var model = await _database.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Select(e => SignupModel.Create(e))
                .SingleOrDefaultAsync(e => e.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            model.User = await _database.Users
                .Include(u => u.Payments)
                .Include(u => u.EventSignups)
                .AsNoTracking()
                .SingleUser(_userManager.GetUserId(User));

            if (model.User.EventSignups.FirstOrDefault(e => e.EventId == id) is EventSignup eventSignup)
            {
                model.UserEventSignup = eventSignup;
                return View("Status", model);
            }

            if (model.Options.SignupOpensAt > DateTime.Now)
            {
                return View("NotOpenYet", model);
            }

            if (model.Options.SignupClosesAt < DateTime.Now)
            {
                return View("ClosedAlready", model);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(Guid id, [FromForm] SignupInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new { id });
            }

            var user = await _database.Users
                .Include(u => u.EventSignups)
                .SingleUser(_userManager.GetUserId(User));

            user.EventSignups.Add(new EventSignup
            {
                EventId = id,
                Priority = 1,
                Role = input.Role,
                PartnerEmail = input.PartnerEmail,
                Status = Status.Pending,
                SignedUpAt = DateTime.UtcNow
            });

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(ThankYou), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> ThankYou(Guid id)
        {
            var model = await _database.Events
                .Expressionify()
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Select(e => SignupModel.Create(e))
                .SingleOrDefaultAsync(e => e.Id == id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptOrReject([FromForm] Guid id, [FromForm] bool accept)
        {
            var user = await _database.Users
                .Include(u => u.EventSignups)
                .SingleUser(_userManager.GetUserId(User));

            var signup = user.EventSignups.FirstOrDefault(s => s.EventId == id);

            if (signup?.Status == Status.Approved)
            {
                signup.Status = accept ? Status.AcceptedAndPayed : Status.RejectedOrNotPayed;
                await _database.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { id });
        }
    }
}