using System;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

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
            if (charge.Metadata.TryGetValue("email", out var email)
            && charge.Metadata.TryGetValue("name", out var name))
            {
                var user = await _memberContext.Users
                    .Include(u => u.Payments)
                    .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());

                if (user == null)
                {
                    await _userManager.CreateAsync(new MemberUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = name,
                        Payments = new List<Payment>
                        {
                            CreatePayment(charge)
                        }
                    });
                }
                else
                {
                    if (user.Payments.FirstOrDefault(p => p.StripeChargeId == charge.Id) is Payment existingPayment)
                    {
                        existingPayment.Refunded = charge.Refunded;
                    }
                    else
                    {
                        user.Payments.Add(CreatePayment(charge));
                    }
                }

                await _memberContext.SaveChangesAsync();
            }
        }

        private static Payment CreatePayment(Charge charge)
        {
            var (includesMembership, includesTraining, includesClasses) = GetIncludedFees(charge.Description);

            return new Payment
            {
                Amount = charge.Amount/100m,
                Description = charge.Description,
                PayedAtUtc = charge.Created,
                StripeChargeId = charge.Id,
                IncludesMembership = charge.Metadata.TryGetValue("inc_membership", out var m) && m == "yes" || includesMembership,
                IncludesTraining = charge.Metadata.TryGetValue("inc_training", out var t) && t == "yes" || includesTraining,
                IncludesClasses = charge.Metadata.TryGetValue("inc_classes", out var c) && c == "yes" || includesClasses,
                Refunded = charge.Refunded
            };
        }

        private static readonly Dictionary<string, (bool?, bool?, bool?)> DescriptionMap = new Dictionary<string, (bool?, bool?, bool?)>
        {
            ["medlemskap"] = (true, null, null),
            ["medlemskap+kurs"] = (true, true, true),
            ["medlemskapgratisKurs"] = (true, true, true),
            ["medlemskap+trening"] = (true, true, null),
            ["2018"] = (false, false, false)
        };

        private static (bool, bool, bool) GetIncludedFees(string description)
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
