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

namespace MemberService.Pages.Admin
{
    [Authorize(Roles = Roles.ADMIN)]
    public class AdminController : Controller
    {
        private readonly ChargeService _chargeService;
        private readonly IPaymentService _paymentService;
        private readonly MemberContext _memberContext;

        public AdminController(
            ChargeService chargeService,
            IPaymentService paymentService,
            MemberContext memberContext)
        {
            _chargeService = chargeService;
            _paymentService = paymentService;
            _memberContext = memberContext;
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
