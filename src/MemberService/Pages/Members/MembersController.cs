﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clave.Expressionify;
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

        public async Task<IActionResult> Index(string memberFilter, string trainingFilter, string classesFilter)
        {
            var users = await _memberContext.Users
                .Include(u => u.Payments)
                .Include(u => u.UserRoles)
                .ThenInclude(r => r.Role)
                .AsNoTracking()
                .Expressionify()
                .Where(MemberFilter(memberFilter))
                .Where(TrainingFilter(trainingFilter))
                .Where(ClassesFilter(classesFilter))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(new MembersViewModel
            {
                Users = users
                    .GroupBy(u => u.FullName?.ToUpper().FirstOrDefault() ?? '?', (key, u) => (key, u.ToReadOnlyCollection()))
                    .ToReadOnlyCollection(),
                MemberFilter = memberFilter,
                TrainingFilter = trainingFilter,
                ClassesFilter = classesFilter
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
        public async Task<IActionResult> ToggleRole([FromForm] string email, [FromForm] string role, [FromForm] bool value)
        {
            if (await _userManager.FindByEmailAsync(email) is MemberUser user)
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

        private static Expression<Func<MemberUser, bool>> MemberFilter(string filter)
        {
            switch (filter)
            {
                case "Only":
                    return user => user.HasPayedMembershipThisYear();
                case "Not":
                    return user => !user.HasPayedMembershipThisYear();
                default:
                    return user => true;
            }
        }

        private static Expression<Func<MemberUser, bool>> TrainingFilter(string filter)
        {
            switch (filter)
            {
                case "Only":
                    return user => user.HasPayedTrainingFeeThisSemester();
                case "Not":
                    return user => !user.HasPayedTrainingFeeThisSemester();
                default:
                    return user => true;
            }
        }

        private static Expression<Func<MemberUser, bool>> ClassesFilter(string filter)
        {
            switch (filter)
            {
                case "Only":
                    return user => user.HasPayedClassesFeeThisSemester();
                case "Not":
                    return user => !user.HasPayedClassesFeeThisSemester();
                default:
                    return user => true;
            }
        }
    }
}
