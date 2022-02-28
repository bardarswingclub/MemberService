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
    private readonly IPaymentService _paymentService;

    public PayController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        MemberContext memberContext,
        IPaymentService paymentService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _memberContext = memberContext;
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index(string title, string description, decimal amount, string email = null, string name = null)
    {
        if (title == null || description == null || amount <= 0)
        {
            return NotFound();
        }

        if (email != null && await _userManager.FindByEmailAsync(email) is User user)
        {
            name = user.FullName;
            if (User.Identity.IsAuthenticated)
            {
                if (_userManager.GetUserId(User) != user.Id)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Account/Login", new { email, returnUrl = Request.GetEncodedPathAndQuery(), Area = "Identity" });
                }
            }
            else
            {
                return RedirectToPage("/Account/Login", new { email, returnUrl = Request.GetEncodedPathAndQuery(), Area = "Identity" });
            }
        }

        var sessionId = await _paymentService.CreatePayment(
            name: name,
            email: email,
            title: title,
            description: description,
            amount: amount,
            successUrl: Url.ActionLink(nameof(Success), "Pay", new { title, description, sessionId = "{CHECKOUT_SESSION_ID}" }),
            cancelUrl: Request.GetDisplayUrl());

        return View(new PayModel
        {
            Id = sessionId,
            Name = title,
            Description = description,
            Amount = amount
        });
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
            return RedirectToAction(nameof(HomeController.Fees), "Home");
        }

        var sessionId = await _paymentService.CreatePayment(
            name: user.FullName,
            email: user.Email,
            title: fee.Description,
            description: fee.Description,
            amount: fee.Amount,
            successUrl: Url.ActionLink(nameof(FeePaid), "Pay", new { type, returnUrl, sessionId = "{CHECKOUT_SESSION_ID}" }),
            cancelUrl: Request.GetDisplayUrl(),
            includesMembership: fee.IncludesMembership,
            includesTraining: fee.IncludesTraining,
            includesClasses: fee.IncludesClasses);

        return View(new PayModel
        {
            Id = sessionId
        });
    }

    public async Task<IActionResult> Success(string title, string description, string sessionId)
    {
        await _paymentService.SavePayment(sessionId);

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

        await _paymentService.SavePayment(sessionId);

        TempData.SetSuccessMessage($"{fee.Description} betalt");

        if (string.IsNullOrEmpty(returnUrl))
        {
            return RedirectToAction(nameof(HomeController.Fees), "Home");
        }

        return Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : Redirect("/");
    }

    private async Task<User> GetCurrentUser()
        => await _memberContext.Users
            .Include(x => x.Payments)
            .SingleUser(_userManager.GetUserId(User));
}
