using MemberService.Data;
using MemberService.Pages.Home;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace MemberService.Pages.Pay
{
    public class PayController : Controller
    {
        private readonly SignInManager<MemberUser> _signInManager;
        private readonly UserManager<MemberUser> _userManager;
        private readonly MemberContext _memberContext;
        private readonly IPaymentService _paymentService;
        private readonly SessionService _sessionService;
        private readonly ChargeService _chargeService;

        public PayController(
            SignInManager<MemberUser> signInManager,
            UserManager<MemberUser> userManager,
            MemberContext memberContext,
            IPaymentService paymentService,
            SessionService sessionService,
            ChargeService chargeService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _memberContext = memberContext;
            _paymentService = paymentService;
            _sessionService = sessionService;
            _chargeService = chargeService;
        }

        public async Task<IActionResult> Index(string title, string description, decimal amount, string email = null, string name = null)
        {
            if (title == null || description == null || amount <= 0)
            {
                return NotFound();
            }

            if (email != null && await _userManager.FindByEmailAsync(email) is MemberUser user)
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
        public async Task<IActionResult> Fee([FromForm] string type)
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
                successUrl: Url.ActionLink(nameof(FeePaid), "Pay", new { type, sessionId = "{CHECKOUT_SESSION_ID}" }),
                cancelUrl: Request.GetDisplayUrl(),
                includesMembership: fee.IncludesMembership,
                includesTraining: fee.IncludesTraining,
                includesClasses: fee.IncludesClasses);

            return View(new PayModel
            {
                Id = sessionId
            });
        }

        public async Task<IActionResult> Success(string name, string description, string sessionId)
        {
            await _paymentService.SavePayment(sessionId);

            return View(new PayModel
            {
                Name = name,
                Description = description
            });
        }

        public async Task<IActionResult> FeePaid(string sessionId, string type)
        {
            var user = await GetCurrentUser();

            var (feeStatus, fee) = user.GetFee(type);

            await _paymentService.SavePayment(sessionId);

            TempData["SuccessMessage"] = $"{fee.Description} betalt";

            return RedirectToAction(nameof(HomeController.Fees), "Home");
        }

        private async Task<MemberUser> GetCurrentUser()
            => await _memberContext.Users
                .Include(x => x.Payments)
                .SingleUser(_userManager.GetUserId(User));
    }
}
