namespace MemberService.Pages.Pay;
using System.Threading.Tasks;

using MemberService.Data;
using MemberService.Services;
using MemberService.Services.Vipps.Models;

using Microsoft.AspNetCore.Mvc;

public class VippsCallbackController : Controller
{
    public const string CallbackPrefix = "/vipps/callback";
    private readonly IVippsPaymentService _vippsPaymentService;

    public VippsCallbackController(IVippsPaymentService vippsPaymentService)
    {
        _vippsPaymentService = vippsPaymentService;
    }

    [HttpPost($"{CallbackPrefix}/v2/payments/{{orderId}}")]
    public async Task<IActionResult> Callback(Guid orderId, [FromBody] PaymentCallback body)
    {
        var authToken = Request.Headers.Authorization.ToString();

        if (body.TransactionInfo.Status == "RESERVED")
        {
            await _vippsPaymentService.CapturePayment(orderId, User.GetId(), authToken);
        }

        return Ok();
    }
}
