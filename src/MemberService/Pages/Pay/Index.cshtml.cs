namespace MemberService.Pages.Pay;

using System.Threading.Tasks;

using MemberService.Data;
using MemberService.Services;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IVippsPaymentService _vippsPaymentService;

    public IndexModel(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IStripePaymentService stripePaymentService,
        IVippsPaymentService vippsPaymentService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _stripePaymentService = stripePaymentService;
        _vippsPaymentService = vippsPaymentService;
    }

    [BindProperty(SupportsGet = true)]
    public string Title { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Description { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal Amount { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Email { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Name { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SessionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? OrderId { get; set; }

    [BindProperty]
    public string Method { get; set; }

    public bool Success { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (!string.IsNullOrWhiteSpace(SessionId))
        {
            await _stripePaymentService.SavePayment(SessionId);
            Success = true;
            return Page();
        }

        if (OrderId is Guid)
        {
            Success = await _vippsPaymentService.CompleteReservations(User.GetId());
            return Page();
        }

        if (Title is null || Description is null || Amount <= 0)
        {
            return NotFound();
        }

        if (Email is not null && await _userManager.FindByEmailAsync(Email) is User user)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login", new { Email, returnUrl = Request.GetEncodedPathAndQuery() });
            }
            else if (User.GetId() != user.Id)
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Account/Login", new { Email, returnUrl = Request.GetEncodedPathAndQuery() });
            }
            else
            {
                Name = user.FullName;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Method == "vipps")
        {
            var url = await _vippsPaymentService.InitiatePayment(
                userId: User.GetId(),
                description: Description,
                amount: Amount,
                returnToUrl: Url.PageLink(values: new { Title, Description, OrderId = "{orderId}" }));

            return Redirect(url);
        }
        else
        {
            var url = await _stripePaymentService.CreatePaymentRequest(
                name: Name,
                email: Email,
                title: Title,
                description: Description,
                amount: Amount,
                successUrl: Url.PageLink(values: new { Title, Description, SessionId = "{CHECKOUT_SESSION_ID}" }),
                cancelUrl: Url.PageLink(values: new { Title, Description, Amount, Name, Email }));

            return Redirect(url);
        }
    }
}
