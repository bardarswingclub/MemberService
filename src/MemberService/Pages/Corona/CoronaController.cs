using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace MemberService.Pages.Corona
{
    [Authorize]
    public class CoronaController : Controller
    {
        private readonly MemberContext _database;
        private readonly UserManager<User> _userManager;
        private readonly RefundService _refundService;

        public CoronaController(
            MemberContext database,
            UserManager<User> userManager,
            RefundService refundService)
        {
            _database = database;
            _userManager = userManager;
            _refundService = refundService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _database.Users
                .Include(u => u.Payments)
                .SingleUser(GetUserId());

            var refund = user.GetCoronaRefundablePayments();

            var model = new CoronaModel
            {
                Refund = refund
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Refund()
        {
            var user = await _database.Users
                .Include(u => u.Payments)
                .SingleUser(GetUserId());

            var payments = user.GetCoronaRefundablePayments();

            foreach (var (payment, amount) in payments)
            {
                try
                {
                    await _refundService.CreateAsync(new RefundCreateOptions
                    {
                        Amount = (long?)(amount * 100),
                        Charge = payment.StripeChargeId,
                        Reason = "requested_by_customer",
                        Metadata = new Dictionary<string, string>
                        {
                            ["Description"] = "Korona"
                        }
                    });

                    payment.IncludesTraining = false;
                    payment.IncludesClasses = false;

                    await _database.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return RedirectToAction(nameof(Failure));
                }
            }

            return RedirectToAction(nameof(Success));
        }

        [HttpGet]
        public IActionResult Success() => View();

        [HttpGet]
        public IActionResult Failure() => View();

        private string GetUserId() => _userManager.GetUserId(User);
    }
}