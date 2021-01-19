using System;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using MemberService.Services;
using Microsoft.EntityFrameworkCore;
using Clave.ExtensionMethods;
using MemberService.Auth;
using Clave.Expressionify;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Pages.Admin
{
    [Authorize(nameof(Policy.IsAdmin))]
    public class AdminController : Controller
    {
        private readonly ChargeService _chargeService;
        private readonly IPaymentService _paymentService;
        private readonly MemberContext _memberContext;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;

        public AdminController(
            ChargeService chargeService,
            IPaymentService paymentService,
            MemberContext memberContext,
            UserManager<User> userManager,
            IEmailService emailService)
        {
            _chargeService = chargeService;
            _paymentService = paymentService;
            _memberContext = memberContext;
            _userManager = userManager;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var userRoles = await _memberContext.UserRoles
                .Include(x => x.User)
                .Include(x => x.Role)
                .ToListAsync();

            var roles = userRoles
                .GroupByProp(x => x.Role, x => x.Id)
                .Select(x => (x.Key, x.Select(u => u.User).ToReadOnlyCollection()))
                .ToReadOnlyCollection();

            return View(new AdminModel
            {
                Roles = roles
            });
        }

        [HttpPost]
        public async Task<IActionResult> Import([FromForm] DateTime? after)
        {
            string lastCharge = null;
            var importedCount = 0;
            var userCount = 0;
            var paymentCount = 0;
            var updatedCount = 0;
            while (true)
            {
                var charges = await _chargeService.ListAsync(new ChargeListOptions
                {
                    Created = new DateRangeOptions
                    {
                        GreaterThan = after ?? new DateTime(2019, 1, 1)
                    },
                    Limit = 100,
                    StartingAfter = lastCharge
                });

                importedCount += charges.Count();
                var (users, payments, updates) = await _paymentService.SavePayments(charges);
                userCount += users;
                paymentCount += payments;
                updatedCount += updates;

                if (!charges.HasMore) break;

                lastCharge = charges.Data.Last().Id;
            }

            TempData["SuccessMessage"] = $"Found {importedCount} payments, created {userCount} new users, saved {paymentCount} new payments and updated {updatedCount} existing payments";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MassEmail([FromForm] string subject, [FromForm] string body, [FromForm] bool onlyMe)
        {
            var members = await _memberContext
                .Users
                .Expressionify()
                .Where(u => onlyMe ? u.Id == GetUserId() : u.HasPayedMembershipLastYear())
                .ToListAsync();

            var successes = new List<string>();
            var failures = new List<string>();

            foreach (var user in members)
            {
                try
                {
                    await _emailService.SendCustomEmail(
                        to: user,
                        subject: subject,
                        message: body);
                    successes.Add(user.Email);
                }
                catch (Exception e)
                {
                    failures.Add($"{user.Email} - {e.Message}");
                }
            }

            if (members.NotAny())
            {
                TempData["InfoMessage"] = "No emails to send";
            }

            TempData["SuccessMessage"] = string.Join(",\n ", successes);
            TempData["ErrorMessage"] = string.Join(",\n ", failures);
            return RedirectToAction(nameof(Index));
        }


        private string GetUserId() => _userManager.GetUserId(User);
    }
}
