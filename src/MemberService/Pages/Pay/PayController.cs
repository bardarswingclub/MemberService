namespace MemberService.Pages.Pay;

using MemberService.Data;
using MemberService.Pages.Home;
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

    public PayController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        MemberContext memberContext,
        IStripePaymentService stripePaymentService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _memberContext = memberContext;
        _stripePaymentService = stripePaymentService;
    }

    [HttpPost]
    [Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Fee([FromForm] string type, string returnUrl = null)
    {
        var user = await GetCurrentUser();

        var (feeStatus, fee) = user.GetFee(type);

        if (feeStatus != FeeStatus.Unpaid)
        {
            return RedirectToPage("/Home/Fees");
        }

        var url = await _stripePaymentService.CreatePaymentRequest(
            name: user.FullName,
            email: user.Email,
            title: fee.Description,
            description: fee.Description,
            amount: fee.Amount,
            successUrl: Url.ActionLink(nameof(FeePaid), "Pay", new { type, returnUrl, sessionId = "{CHECKOUT_SESSION_ID}" }),
            cancelUrl: returnUrl,
            includesMembership: fee.IncludesMembership,
            includesTraining: fee.IncludesTraining,
            includesClasses: fee.IncludesClasses);

        return Redirect(url);
    }

    public async Task<IActionResult> Success(string title, string description, string sessionId)
    {
        await _stripePaymentService.SavePayment(sessionId);

        return View(new PayModel
        {
            Name = title,
            Description = description
        });
    }

    public async Task<IActionResult> FeePaid(string sessionId, string type, string returnUrl = null)
    {
        var user = await GetCurrentUser();

        var (feeStatus, fee) = user.GetFee(type);

        await _stripePaymentService.SavePayment(sessionId);

        TempData.SetSuccessMessage($"{fee.Description} betalt");

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
            .SingleUser(User.GetId());
}
