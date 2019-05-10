using MemberService.Data;
using MemberService.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MemberService.Pages.Pay
{
    public class PayController : Controller
    {
        private readonly SignInManager<MemberUser> _signInManager;
        private readonly UserManager<MemberUser> _userManager;
        private readonly IPaymentService _paymentService;
        private readonly SessionService _sessionService;
        private readonly ChargeService _chargeService;

        public PayController(
            SignInManager<MemberUser> signInManager,
            UserManager<MemberUser> userManager,
            IPaymentService paymentService,
            SessionService sessionService,
            ChargeService chargeService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _paymentService = paymentService;
            _sessionService = sessionService;
            _chargeService = chargeService;
        }

        public async Task<IActionResult> Index(string title, string description, decimal amount, string email=null, string name=null)
        {
            if(title == null || description == null || amount <= 0)
            {
                return NotFound();
            }

            if(email != null && await _userManager.FindByEmailAsync(email) is MemberUser user)
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

            var session = await _sessionService.CreateAsync(new SessionCreateOptions
            {
                CustomerEmail = email,
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Description = $"{title} ({description})",
                    Metadata = new Dictionary<string, string>
                    {
                        ["name"] = name,
                        ["email"] = email,
                        ["amount_owed"] = amount.ToString(),
                        ["long_desc"] = description,
                        ["short_desc"] = title
                    }
                },
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Name = title,
                        Description = description,
                        Amount = (long) amount*100L,
                        Currency = "nok",
                        Quantity = 1
                    }
                },
                SuccessUrl = Url.Action(nameof(Success), "Pay", new { title, description }, Request.Scheme, Request.Host.Value),
                CancelUrl = Request.GetDisplayUrl(),
            });

            TempData["StripeSessionId"] = session.Id;

            return View(new PayModel
            {
                Id = session.Id,
                Name = title,
                Description = description,
                Amount = amount
            });
        }

        public async Task<IActionResult> Success(string name, string description)
        {
            if (TempData["StripeSessionId"] is string sessionId) {
                var session = await _sessionService.GetAsync(sessionId);

                var charges = await _chargeService.ListAsync(new ChargeListOptions
                {
                    PaymentIntentId = session.PaymentIntentId
                });

                await _paymentService.SavePayments(charges);
            }

            return View(new PayModel
            {
                Name = name,
                Description = description
            });
        }
    }
}
