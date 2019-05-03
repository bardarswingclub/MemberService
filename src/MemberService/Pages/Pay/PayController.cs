using MemberService.Data;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MemberService.Pages.Pay
{
    public class PayController : Controller
    {
        private readonly SignInManager<MemberUser> _signInManager;
        private readonly UserManager<MemberUser> _userManager;

        public PayController(
            SignInManager<MemberUser> signInManager,
            UserManager<MemberUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string name, string description, decimal amount, string email=null)
        {
            if(name == null || description == null || amount <= 0)
            {
                return NotFound();
            }

            if (email != null)
            {
                if(await _userManager.FindByEmailAsync(email) is MemberUser user)
                {
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
            }

            var options = new SessionCreateOptions
            {
                CustomerEmail = email,
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Description = description
                },
                PaymentMethodTypes = new List<string> {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions> {
                    new SessionLineItemOptions {
                        Name = name,
                        Description = description,
                        Amount = (long) amount*100L,
                        Currency = "nok",
                        Quantity = 1,
                    },
                },
                SuccessUrl = Url.Action(nameof(Success), "Pay", new { name, description }, Request.Scheme, Request.Host.Value),
                CancelUrl = Request.GetDisplayUrl(),
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return View(new PayModel
            {
                Id = session.Id,
                Name = name,
                Description = description,
                Amount = amount
            });
        }

        public IActionResult Success(string name, string description)
        {
            return View(new PayModel
            {
                Name = name,
                Description = description
            });
        }
    }
}
