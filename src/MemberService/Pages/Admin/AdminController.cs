using System;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using MemberService.Services;

namespace MemberService.Pages.Admin
{
    [Authorize(Roles = Roles.ADMIN)]
    public class AdminController : Controller
    {
        private readonly ChargeService _chargeService;
        private readonly IPaymentService _paymentService;

        public AdminController(
            ChargeService chargeService,
            IPaymentService paymentService)
        {
            _chargeService = chargeService;
            _paymentService = paymentService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import([FromForm] DateTime? after)
        {
            string lastCharge = null;
            var importedCount = 0;
            var updatedCount = 0;
            while (true)
            {
                var charges = await _chargeService.ListAsync(new ChargeListOptions
                {
                    CreatedRange = new DateRangeOptions
                    {
                        GreaterThan = after ?? new DateTime(2019, 1, 1)
                    },
                    Limit = 100,
                    StartingAfter = lastCharge
                });

                importedCount += charges.Count();
                updatedCount += await _paymentService.SavePayments(charges);

                if (!charges.HasMore) break;

                lastCharge = charges.Data.Last().Id;
            }

            TempData["message"] = $"Imported {importedCount} payments and saved {updatedCount} payments";
            return RedirectToAction(nameof(Index));
        }
    }
}
