﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Clave.ExtensionMethods;
using MemberService.Data;
using MemberService.Pages.Event;
using MemberService.Pages.Signup;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Home
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MemberContext _memberContext;
        private readonly UserManager<MemberUser> _userManager;

        public HomeController(
            MemberContext memberContext,
            UserManager<MemberUser> userManager)
        {
            _memberContext = memberContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var openClasses = await _memberContext.GetClasses(userId, e => e.IsOpen());

            var futureClasses = await _memberContext.GetClasses(userId, e => e.WillOpen());

            var willOpenAt = futureClasses
                .WhereNotNull(e => e.OpensAt)
                .OrderBy(e => e.OpensAt)
                .Select(e => e.OpensAt)
                .FirstOrDefault();

            return View(new HomeModel
            {
                Classes = openClasses,
                OpensAt = willOpenAt
            });
        }

        [HttpPost]
        public async Task<IActionResult> Signup(
            [FromForm] IReadOnlyList<Guid> classes,
            [FromForm] IReadOnlyList<DanceRole> roles,
            [FromForm] IReadOnlyList<string> partners,
            [FromForm] Guid? accept=null,
            [FromForm] Guid? reject=null)
        {
            if(accept is Guid id)
            {

            }

            var items = new List<ClassSignup>();
            for (int i = 0; i < classes.Count; i++)
            {
                items.Add(new ClassSignup(classes[i], roles[i], partners[i], i));
            }

            var userId = _userManager.GetUserId(User);
            var user = await _memberContext.GetEditableUser(userId);

            var openClasses = await _memberContext.GetClasses(userId, e => e.IsOpen());

            var classesNotSignedUpFor = openClasses
                .Where(c => c.Signup == null)
                .Select(c => c.Id)
                .ToReadOnlyList();

            var addedSignups = items
                .Where(i => classesNotSignedUpFor.Contains(i.Id))
                .ToReadOnlyList();

            foreach (var signup in addedSignups)
            {
                user.AddEventSignup(signup.Id, signup.Role, signup.PartnerEmail, false, signup.Priority);
            }

            var changedSignups = openClasses
                .WhereNotNull(c => c.Signup)
                .Join(items, c => c.Id, c => c.Id)
                .ToReadOnlyList();

            foreach(var (_, signup) in changedSignups)
            {
                var eventSignup = user.EventSignups.FirstOrDefault(s => s.EventId == signup.Id);
                eventSignup.Priority = signup.Priority;
            }

            var removedSignups = openClasses
                .WhereNotNull(c => c.Signup)
                .Where(c => items.NotAny(i => i.Id == c.Id))
                .ToReadOnlyList();

            await _memberContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Fees()
        {
            var user = await GetCurrentUser();

            return View(new FeesViewModel
            {
                MembershipFee = user.GetMembershipFee(),
                TrainingFee = user.GetTrainingFee(),
                ClassesFee = user.GetClassesFee()
            });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StatusCode(string statusCode)
        {
            return View();
        }

        private async Task<MemberUser> GetCurrentUser()
            => await _memberContext.Users
                .Include(x => x.Payments)
                .SingleUser(_userManager.GetUserId(User));

        private class ClassSignup
        {
            public ClassSignup(Guid id, DanceRole role, string partnerEmail, int priority)
            {
                Id = id;
                Role = role;
                PartnerEmail = partnerEmail;
                Priority = priority;
            }

            public Guid Id { get; }

            public DanceRole Role { get; }

            public string PartnerEmail { get; }

            public int Priority { get; }
        }
    }
}
