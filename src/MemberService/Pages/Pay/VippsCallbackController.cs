namespace MemberService.Pages.Pay;
using System.Threading.Tasks;

using MemberService.Services.Vipps;
using MemberService.Services.Vipps.Models;

using Microsoft.AspNetCore.Mvc;

public class VippsCallbackController : Controller
{
    public const string CallbackPrefix = "/vipps/callback";
    private readonly IVippsClient _vippsClient;

    public VippsCallbackController(IVippsClient vippsClient)
    {
        _vippsClient = vippsClient;
    }

    [HttpPost($"{CallbackPrefix}/v2/payments/{{orderId}}")]
    public async Task<IActionResult> Callback(string orderId, [FromBody] PaymentCallback body)
    {
        var authToken = Request.Headers.Authorization.ToString();

        // get from database
        // check authToken
        var transactionText = "Testing"; // get from database

        await _vippsClient.CapturePayment(orderId, transactionText);

        // save to database

        return Ok();
    }
}
