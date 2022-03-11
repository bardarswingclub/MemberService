namespace MemberService.Pages.Admin;

using Microsoft.AspNetCore.Mvc;
using Stripe;

using Microsoft.AspNetCore.Authorization;
using MemberService.Services;
using MemberService.Auth;

[Authorize(nameof(Policy.IsAdmin))]
public class AdminController : Controller
{
    private readonly ChargeService _chargeService;
    private readonly IStripePaymentService _stripePaymentService;

    public AdminController(
        ChargeService chargeService,
        IStripePaymentService stripePaymentService)
    {
        _chargeService = chargeService;
        _stripePaymentService = stripePaymentService;
    }

    public IActionResult Index() => View();

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
            var (users, payments, updates) = await _stripePaymentService.SavePayments(charges);
            userCount += users;
            paymentCount += payments;
            updatedCount += updates;

            if (!charges.HasMore) break;

            lastCharge = charges.Data.Last().Id;
        }

        TempData.SetSuccessMessage($"Found {importedCount} payments, created {userCount} new users, saved {paymentCount} new payments and updated {updatedCount} existing payments");
        return RedirectToAction(nameof(Index));
    }
}
