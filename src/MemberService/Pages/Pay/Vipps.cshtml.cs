namespace MemberService.Pages.Pay;
using System;
using System.Threading.Tasks;

using MemberService.Data;
using MemberService.Services;
using MemberService.Services.Vipps;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class VippsModel : PageModel
{
    private readonly IVippsPaymentService _vippsPaymentService;

    public VippsModel(
        IVippsPaymentService vippsPaymentService)
    {
        _vippsPaymentService = vippsPaymentService;
    }

    public bool Success { get; set; }

    public async Task<IActionResult> OnGet(Guid? orderId = null)
    {
        if (orderId is Guid id)
        {
            Success = await _vippsPaymentService.CompleteReservations(User.GetId());
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var url = await _vippsPaymentService.InitiatePayment(
            userId: User.GetId(),
            description:"Testing",
            amount: 200,
            returnToUrl: Url.PageLink(values: new { orderId = "{orderId}" }),
            includesMembership: true,
            includesClasses: true,
            includesTraining: true);

        return Redirect(url);
    }

    public async Task<IActionResult> OnGetDetails(Guid orderId, [FromServices] IVippsClient vippsClient)
    {
        var details = await vippsClient.GetPaymentDetails(orderId.ToString());

        return new JsonResult(details);
    }
}
