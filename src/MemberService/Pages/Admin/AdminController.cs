using System;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using Clave.ExtensionMethods;

namespace MemberService.Pages.Admin
{
    public class AdminController : Controller
    {
        private readonly ChargeService _chargeService;
        private readonly UserManager<MemberUser> _userManager;
        private readonly MemberContext _memberContext;

        public AdminController(
            ChargeService chargeService,
            UserManager<MemberUser> userManager,
            MemberContext memberContext)
        {
            _chargeService = chargeService;
            _userManager = userManager;
            _memberContext = memberContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import([FromForm] DateTime? after)
        {
            string lastCharge = null;
            var updatedCount = 0;
            while (true)
            {
                var charges = await _chargeService.ListAsync(new ChargeListOptions
                {
                    CreatedRange = new DateRangeOptions
                    {
                        GreaterThan = after ?? new DateTime(2019, 1, 1)
                    },
                    Limit = 100,
                    StartingAfter = lastCharge
                });

                foreach (var charge in charges)
                {
                    await SavePayment(charge);
                    updatedCount++;
                }

                if (!charges.HasMore) break;

                lastCharge = charges.Data.Last().Id;
            }

            TempData["message"] = $"Imported {updatedCount} payments";
            return RedirectToAction(nameof(Index));
        }

        private async Task SavePayment(Charge charge)
        {
            var email = charge.Metadata["email"];
            var name = charge.Metadata["name"];
            var user = await _memberContext.Users
                .Include(u => u.Payments)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());

            var (includesMembership, includesTraining, includesClasses) = GetIncludedFees(charge.Description);

            var payment = new Payment
            {
                Amount = charge.Amount,
                Description = charge.Description,
                PayedAt = charge.Created,
                StripeChargeId = charge.Id,
                IncludesMembership = charge.Metadata.TryGetValue("inc_membership", out var m) && m == "yes" || includesMembership,
                IncludesTraining = charge.Metadata.TryGetValue("inc_training", out var t) && t == "yes" || includesTraining,
                IncludesClasses = charge.Metadata.TryGetValue("inc_classes", out var c) && c == "yes" || includesClasses,
            };

            if (user == null)
            {
                await _userManager.CreateAsync(new MemberUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name,
                    Payments = new List<Payment>
                    {
                        payment
                    }
                });
            }
            else
            {
                if (user.Payments.NotAny(p => p.StripeChargeId == charge.Id))
                {
                    user.Payments.Add(payment);
                }
            }

            await _memberContext.SaveChangesAsync();
        }

        private readonly Dictionary<string, (bool?, bool?, bool?)> DescriptionMap = new Dictionary<string, (bool?, bool?, bool?)>
        {
            ["medlemskap"] = (true, null, null),
            ["medlemskap+kurs"] = (true, true, true),
            ["medlemskapgratisKurs"] = (true, true, true),
            ["medlemskap+trening"] = (true, true, null),
            ["2018"] = (false, false, false)
        };

        private (bool, bool, bool) GetIncludedFees(string description)
        {
            var membership = false;
            var training = false;
            var classes = false;
            foreach (var (d, (m, t, c)) in DescriptionMap)
            {
                if (description.Contains(d, StringComparison.OrdinalIgnoreCase))
                {
                    membership = m ?? membership;
                    training = t ?? training;
                    classes = c ?? classes;
                }
            }

            return (membership, training, classes);
        }
    }
}
