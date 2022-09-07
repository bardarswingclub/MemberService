namespace MemberService.Pages.Pay;

using Clave.ExtensionMethods;

using MemberService.Data;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PayController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly MemberContext _memberContext;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IVippsPaymentService _vippsPaymentService;

    public PayController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        MemberContext memberContext,
        IStripePaymentService stripePaymentService,
        IVippsPaymentService vippsPaymentService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _memberContext = memberContext;
        _stripePaymentService = stripePaymentService;
        _vippsPaymentService = vippsPaymentService;
    }

    [HttpPost]
    [Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Fee([FromForm] string type, [FromForm] string method, string returnUrl = null)
    {
        var user = await GetCurrentUser();

        var (feeStatus, fee) = user.GetFee(type);

        if (feeStatus != FeeStatus.Unpaid)
        {
            return RedirectToPage("/Home/Fees");
        }
        string url = method == "vipps"
            ? await CreateVippsPayment(user, fee, returnUrl ?? Request.Headers.Referer.ToString()?.Pipe(url => new Uri(url).PathAndQuery))
            : await CreateStripePayment(user, fee, returnUrl ?? Request.Headers.Referer.ToString()?.Pipe(url => new Uri(url).PathAndQuery));

        return Redirect(url);
    }

    private async Task<string> CreateVippsPayment(User user, Fee fee, string returnUrl)
    {
        return await _vippsPaymentService.InitiatePayment(
            userId: user.Id,
            description: fee.Description,
            amount: fee.Amount,
            returnToUrl: Url.ActionLink(nameof(FeePaid), values: new { fee.Description, returnUrl, orderId = "{orderId}" }),
            includesMembership: fee.IncludesMembership,
            includesTraining: fee.IncludesTraining,
            includesClasses: fee.IncludesClasses);
    }

    private async Task<string> CreateStripePayment(User user, Fee fee, string returnUrl)
    {
        return await _stripePaymentService.CreatePaymentRequest(
            name: user.FullName,
            email: user.Email,
            title: fee.Description,
            description: fee.Description,
            amount: fee.Amount,
            successUrl: Url.ActionLink(nameof(FeePaid), values: new { fee.Description, returnUrl, sessionId = "{CHECKOUT_SESSION_ID}" }),
            cancelUrl: new Uri(new Uri(Request.GetDisplayUrl()), returnUrl).AbsoluteUri,
            includesMembership: fee.IncludesMembership,
            includesTraining: fee.IncludesTraining,
            includesClasses: fee.IncludesClasses);
    }

    public async Task<IActionResult> FeePaid(string sessionId, string orderId, string description, string returnUrl = null)
    {
        if (sessionId is not null)
        {
            TempData.SetSuccessMessage($"{description} betalt");
            await _stripePaymentService.SavePayment(sessionId);
        }

        if (orderId is not null)
        {
            if (await _vippsPaymentService.CompleteReservations(User.GetId()))
            {
                TempData.SetSuccessMessage($"{description} betalt");
            }
        }

        if (string.IsNullOrEmpty(returnUrl))
        {
            return RedirectToPage("/Home/Fees");
        }

        return Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : Redirect("/");
    }

    private async Task<User> GetCurrentUser()
        => await _memberContext.Users
            .Include(x => x.Payments)
            .SingleUser(User);
}
