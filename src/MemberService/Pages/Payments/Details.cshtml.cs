namespace MemberService.Pages.Payments;

using System.Threading.Tasks;

using MemberService.Data;
using MemberService.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class DetailsModel : PageModel
{
    private readonly MemberContext _database;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IVippsPaymentService _vippsPaymentService;

    public DetailsModel(
        MemberContext database,
        IStripePaymentService stripePaymentService,
        IVippsPaymentService vippsPaymentService)
    {
        _database = database;
        _stripePaymentService = stripePaymentService;
        _vippsPaymentService = vippsPaymentService;
    }

    public Payment Payment { get; set; }

    public async Task OnGetAsync(string id)
    {
        Payment = await _database.Payments.FindAsync(id);
        if (Payment.StripeChargeId is { } stripeId)
        {
            _stripePaymentService.GetPaymentInfo(stripeId);
        }

        if(Payment.VippsOrderId is { } vippsOrderId)
        {
            _vippsPaymentService.GetPaymentInfo(vippsOrderId);
        }
    }
}
