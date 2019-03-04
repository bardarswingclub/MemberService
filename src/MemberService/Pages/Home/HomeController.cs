using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MemberService.Data;
using MemberService.Services;
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

            var fee = user.GetMembershipFee();

            return View(new IndexViewModel
            {
                User = user,
                MembershipFee = fee
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

            var fee = user.GetMembershipFee();

            if (fee == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var options = new ChargeCreateOptions
            {
                Amount = (long)fee.Amount * 100,
                Currency = "nok",
                Description = fee.Description,
                SourceId = payment.stripeToken,
                ReceiptEmail = payment.stripeEmail,
                Metadata = new Dictionary<string, string>
                {
                    ["name"] = user.UserName,
                    ["email"] = user.Email,
                    ["amount_owed"] = fee.Amount.ToString(),
                    ["long_desc"] = fee.Description,
                    ["short_desc"] = fee.Description
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
