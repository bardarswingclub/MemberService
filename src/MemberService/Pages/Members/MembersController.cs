using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clave.Expressionify;
using Clave.ExtensionMethods;
using MemberService.Data;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Members
{
    [Authorize(Roles = Roles.COORDINATOR_OR_ADMIN)]
    public class MembersController : Controller
    {
        private readonly MemberContext _memberContext;
        private readonly UserManager<MemberUser> _userManager;
        private readonly IPaymentService _paymentService;

        public MembersController(
            MemberContext memberContext,
            UserManager<MemberUser> userManager,
            IPaymentService paymentService)
        {
            _memberContext = memberContext;
            _userManager = userManager;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index(
            string memberFilter,
            string trainingFilter,
            string classesFilter,
            string exemptTrainingFilter,
            string exemptClassesFilter)
        {
            var users = await _memberContext.Users
                .Include(u => u.Payments)
                .AsNoTracking()
                .Expressionify()
                .Where(u => u.EmailConfirmed)
                .Where(Filter(memberFilter, u => u.HasPayedMembershipThisYear()))
                .Where(Filter(trainingFilter, u => u.HasPayedTrainingFeeThisSemester()))
                .Where(Filter(classesFilter, u => u.HasPayedClassesFeeThisSemester()))
                .Where(Filter(exemptTrainingFilter, u => u.ExemptFromTrainingFee))
                .Where(Filter(exemptClassesFilter, u => u.ExemptFromClassesFee))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(new MembersViewModel
            {
                Users = users
                    .GroupBy(u => u.FullName?.ToUpper().FirstOrDefault() ?? '?', (key, u) => (key, u.ToReadOnlyCollection()))
                    .ToReadOnlyCollection(),
                MemberFilter = memberFilter,
                TrainingFilter = trainingFilter,
                ClassesFilter = classesFilter,
                ExemptTrainingFilter = exemptTrainingFilter,
                ExemptClassesFilter = exemptClassesFilter
            });
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _memberContext.Users
                .Include(u => u.Payments)
                .Include(u => u.UserRoles)
                    .ThenInclude(r => r.Role)
                .Include(u => u.EventSignups)
                    .ThenInclude(s => s.Event)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<IActionResult> ToggleRole(string id, [FromForm] string role, [FromForm] bool value)
        {
            if (await _userManager.FindByIdAsync(id) is MemberUser user)
            {
                if (value && !await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
                else if (!value && await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }


                return RedirectToAction(nameof(Details), new { id });
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<IActionResult> SetExemptions(string id, [FromForm] bool exemptFromTrainingFee, [FromForm] bool exemptFromClassesFee)
        {
            if (await _memberContext.FindAsync<MemberUser>(id) is MemberUser user)
            {
                user.ExemptFromTrainingFee = exemptFromTrainingFee;
                user.ExemptFromClassesFee = exemptFromClassesFee;

                await _memberContext.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id });
            }

            return NotFound();
        }

        [HttpPost]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<IActionResult> AddManualPayment(string id, [FromForm] ManualPaymentModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _memberContext.FindAsync<MemberUser>(id) is MemberUser user)
            {
                await _memberContext.AddAsync(new Payment
                {
                    User = user,
                    Amount = model.Amount,
                    Description = model.Description,
                    IncludesMembership = model.IncludesMembership,
                    IncludesTraining = model.IncludesTraining,
                    IncludesClasses = model.IncludesClasses,
                    PayedAtUtc = DateTime.UtcNow,
                    ManualPayment = User.Identity.Name
                });

                await _memberContext.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePayments(string id)
        {
            if (await _memberContext.FindAsync<MemberUser>(id) is MemberUser user)
            {
                var (payments, updates) = await _paymentService.ImportPayments(user.NormalizedEmail);

                TempData["SuccessMessage"] = $"Fant {payments} nye betalinger, oppdaterte {updates} eksisterende betalinger";

                return RedirectToAction(nameof(Details), new { id });
            }

            return NotFound();
        }

        private static Expression<Func<MemberUser, bool>> Filter(string filter, Expression<Func<MemberUser, bool>> predicate)
        {
            switch (filter)
            {
                case "Only":
                    return predicate;
                case "Not":
                    return predicate.Not();
                default:
                    return user => true;
            }
        }
    }
}
