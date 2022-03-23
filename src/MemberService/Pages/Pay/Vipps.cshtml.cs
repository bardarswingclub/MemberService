namespace MemberService.Pages.Pay;
using System;
using System.Threading.Tasks;

using MemberService.Data;
using MemberService.Services;
using MemberService.Services.Vipps;
using MemberService.Services.Vipps.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class VippsModel : PageModel
{
    private readonly IVippsClient _vippsClient;
    private readonly IVippsPaymentService _vippsPaymentService;

    public VippsModel(
        IVippsClient vippsClient,
        IVippsPaymentService vippsPaymentService)
    {
        _vippsClient = vippsClient;
        _vippsPaymentService = vippsPaymentService;
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
        var url = await _vippsPaymentService.InitiatePayment(
            userId: User.GetId(),
            description:"Testing",
            amount: 200,
            successUrl: Url.PageLink(values: new { orderId = "{orderId}" }),//"http://127.0.0.1:5862/Pay/Vipps/{orderId}",
            callbackUrl: "https://eohgrfxlsp9eu05.m.pipedream.net",
            includesMembership: true,
            includesClasses: true,
            includesTraining: true);

        return Redirect(url);
    }

    public async Task<IActionResult> OnPostCapture(Guid orderId)
    {
        await _vippsPaymentService.CapturePayment(orderId, User.GetId());

        return Page();
    }
}
