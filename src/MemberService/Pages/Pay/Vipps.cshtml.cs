namespace MemberService.Pages.Pay;
using System;
using System.Threading.Tasks;

using MemberService.Services.Vipps;
using MemberService.Services.Vipps.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class VippsModel : PageModel
{
    private readonly IVippsClient _vippsClient;

    public VippsModel(IVippsClient vippsClient)
    {
        _vippsClient = vippsClient;
    }

    public PaymentDetails PaymentDetails { get; set; }

    public async Task<IActionResult> OnGet(string orderId = null)
    {
        if (orderId is not null)
        {
            PaymentDetails = await _vippsClient.GetPaymentDetails(orderId);
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var orderId = Guid.NewGuid().ToString();
        var paymentResponse = await _vippsClient.InitiatePayment(new()
        {
            OrderId = orderId,
            Amount = 200 * 100,
            TransactionText = "Testing"
        }, "http://127.0.0.1:5862/Pay/Vipps");

        return Redirect(paymentResponse.Url);
    }

    public async Task<IActionResult> OnPostCapture(string orderId)
    {
        var paymentResponse = await _vippsClient.CapturePayment(orderId, "Testing");

        return Page();
    }
}
