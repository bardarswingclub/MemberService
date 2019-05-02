using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemberService.Pages.Pay
{
    public class PayController : Controller
    {
        public async Task<IActionResult> Index(string name, string description, decimal amount)
        {
            if(name == null || description == null || amount <= 0)
            {
                return NotFound();
            }

            var options = new SessionCreateOptions
            {
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
