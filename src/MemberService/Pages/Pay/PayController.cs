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
    private readonly INewsletterService _newsletterService;

    public PayController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        MemberContext memberContext,
        IStripePaymentService stripePaymentService,
        IVippsPaymentService vippsPaymentService,
        INewsletterService newsletterService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _memberContext = memberContext;
        _stripePaymentService = stripePaymentService;
        _vippsPaymentService = vippsPaymentService;
        _newsletterService = newsletterService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Fee([FromForm] string type, [FromForm] string method, [FromForm] bool newsletter, string returnUrl = null)
    {
        var user = await GetCurrentUser();

        var (feeStatus, fee) = user.GetFee(type);

        if (feeStatus != FeeStatus.Unpaid)
        {
            return RedirectToPage("/Home/Fees");
        }
        // Påmeldingen skjer først når betalingen er gjennomført (se FeePaid), så vi sender bare med et flagg
        newsletter = newsletter && fee.IncludesMembership;

        string url = method == "vipps"
            ? await CreateVippsPayment(user, fee, returnUrl ?? Request.Headers.Referer.ToString()?.Pipe(url => new Uri(url).PathAndQuery), newsletter)
            : await CreateStripePayment(user, fee, returnUrl ?? Request.Headers.Referer.ToString()?.Pipe(url => new Uri(url).PathAndQuery), newsletter);

        return Redirect(url);
    }

    private async Task<string> CreateVippsPayment(User user, Fee fee, string returnUrl, bool newsletter)
    {
        return await _vippsPaymentService.InitiatePayment(
            userId: user.Id,
            description: fee.Description,
            amount: fee.Amount,
            returnToUrl: Url.ActionLink(nameof(FeePaid), values: new { fee.Description, returnUrl, newsletter, orderId = "{orderId}" }),
            includesMembership: fee.IncludesMembership,
            includesTraining: fee.IncludesTraining,
            includesClasses: fee.IncludesClasses);
    }

    private async Task<string> CreateStripePayment(User user, Fee fee, string returnUrl, bool newsletter)
    {
        return await _stripePaymentService.CreatePaymentRequest(
            name: user.FullName,
            email: user.Email,
            title: fee.Description,
            description: fee.Description,
            amount: fee.Amount,
            successUrl: Url.ActionLink(nameof(FeePaid), values: new { fee.Description, returnUrl, newsletter, sessionId = "{CHECKOUT_SESSION_ID}" }),
            cancelUrl: new Uri(new Uri(Request.GetDisplayUrl()), returnUrl).AbsoluteUri,
            includesMembership: fee.IncludesMembership,
            includesTraining: fee.IncludesTraining,
            includesClasses: fee.IncludesClasses);
    }

    public async Task<IActionResult> FeePaid(string sessionId, string orderId, string description, string returnUrl = null, bool newsletter = false)
    {
        var paid = false;

        if (sessionId is not null)
        {
            TempData.SetSuccessMessage($"{description} betalt");
            await _stripePaymentService.SavePayment(sessionId);
            paid = true;
        }

        if (orderId is not null)
        {
            if (await _vippsPaymentService.CompleteReservations(User.GetId()))
            {
                TempData.SetSuccessMessage($"{description} betalt");
                paid = true;
            }
        }

        if (paid && newsletter)
        {
            SubscribeToNewsletter(await GetCurrentUser());
        }

        if (string.IsNullOrEmpty(returnUrl))
        {
            return RedirectToPage("/Home/Fees");
        }

        return Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : Redirect("/");
    }

    /// <summary>Kjøres ved siden av forespørselen, slik at et tregt eller nede Mailchimp aldri forsinker eller avbryter betalingen.</summary>
    private void SubscribeToNewsletter(User user)
    {
        var (email, fullName) = (user.Email, user.FullName);
        _ = Task.Run(async () =>
        {
            try
            {
                await _newsletterService.Subscribe(email, fullName);
            }
            catch (Exception)
            {
                // Nyhetsbrevet er en bonus og skal aldri påvirke betalingen
            }
        });
    }

    private async Task<User> GetCurrentUser()
        => await _memberContext.Users
            .Include(x => x.Payments)
            .SingleUser(User);
}
