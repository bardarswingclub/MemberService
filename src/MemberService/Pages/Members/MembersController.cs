using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clave.ExtensionMethods;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

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

        public async Task<IActionResult> Index(string filter)
        {
            var users = await _memberContext.Users
                .Include(u => u.Payments)
                .Include(u => u.UserRoles)
                .ThenInclude(r => r.Role)
                .AsNoTracking()
                .Where(Filter(filter))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(new MembersViewModel
            {
                Users = users
                    .GroupBy(u => u.FullName[0], (key, u) => (key, u?.ToReadOnlyCollection() ?? Empty.ReadOnlyCollection<MemberUser>()))
                    ?.ToReadOnlyCollection() ?? Empty.ReadOnlyCollection<(char, IReadOnlyCollection<MemberUser>)>(),
                OnlyMembers = filter == "OnlyMembers",
                OnlyTraining = filter == "OnlyTraining",
                OnlyClasses = filter == "OnlyClasses"
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

        [Authorize(Roles = Roles.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> MakeAdmin([FromForm]string email)
        {
            await _userManager.EnsureUserHasRole(email, Roles.ADMIN);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Roles.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> MakeCoordinator([FromForm]string email)
        {
            await _userManager.EnsureUserHasRole(email, Roles.COORDINATOR);

            return RedirectToAction(nameof(Index));
        }

        private static Expression<Func<MemberUser, bool>> Filter(string filter)
        {
            switch (filter)
            {
                case "OnlyMembers":
                    return Extensions.HasPayedMembershipThisYearExpression;
                case "OnlyTraining":
                    return Extensions.HasPayedTrainingThisSemesterExpression;
                case "OnlyClasses":
                    return Extensions.HasPayedClassesThisSemesterExpression;
                default:
                    return user => true;
            }
        }
    }
}
