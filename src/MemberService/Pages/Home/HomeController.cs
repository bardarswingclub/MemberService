using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace MemberService.Pages.Home
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MemberContext _memberContext;
        private readonly UserManager<MemberUser> _userManager;
        private readonly ChargeService _stripeChargeService;

        public HomeController(
            MemberContext memberContext,
            UserManager<MemberUser> userManager,
            ChargeService stripeChargeService)
        {
            _memberContext = memberContext;
            _userManager = userManager;
            _stripeChargeService = stripeChargeService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();

            return base.View(new IndexViewModel
            {
                User = user,
                HasPayedMembershipThisYear = user.HasPayedMembershipThisYear()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay([FromForm] StripePayment payment)
        {
            var user = await GetCurrentUser();

            if (payment.stripeEmail != user.Email)
            {
                throw new Exception("Who is this email for???");
            }

            var options = new ChargeCreateOptions
            {
                Amount = payment.Amount,
                Currency = "nok",
                Description = "medlemskap",
                SourceId = payment.stripeToken,
                ReceiptEmail = payment.stripeEmail,
                Metadata = new Dictionary<string, string>
                {
                    ["name"] = user.UserName,
                    ["email"] = user.Email,
                    ["amount_owed"] = payment.Amount.ToString(),
                    ["long_desc"] = "Kun medlemskap",
                    ["short_desc"] = "medlemskap"
                }
            };

            var charge = await _stripeChargeService.CreateAsync(options);

            _memberContext.Payments.Add(new Payment
            {
                User = user,
                PayedAt = DateTime.Now,
                StripeChargeId = charge.Id,
                Amount = charge.Amount,
                Description = charge.Description,
                IncludesMembership = true
            });
            await _memberContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<MemberUser> GetCurrentUser()
            => await _memberContext.GetUser(_userManager.GetUserId(User));
    }
}
