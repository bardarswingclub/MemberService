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

        public MembersController(
            MemberContext memberContext,
            UserManager<MemberUser> userManager)
        {
            _memberContext = memberContext;
            _userManager = userManager;
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
                .Where(Filter(memberFilter, user => user.HasPayedMembershipThisYear()))
                .Where(Filter(trainingFilter, user => user.HasPayedTrainingFeeThisSemester()))
                .Where(Filter(classesFilter, user => user.HasPayedClassesFeeThisSemester()))
                .Where(Filter(exemptTrainingFilter, user => user.ExemptFromTrainingFee))
                .Where(Filter(exemptClassesFilter, user => user.ExemptFromClassesFee))
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


                return RedirectToAction(nameof(Details), new { id = user.Id });
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

                return RedirectToAction(nameof(Details), new { id = user.Id });
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
