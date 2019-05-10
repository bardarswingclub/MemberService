using MemberService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly UserManager<MemberUser> _userManager;
        private readonly MemberContext _memberContext;

        public PaymentService(
            UserManager<MemberUser> userManager,
            MemberContext memberContext)
        {
            _userManager = userManager;
            _memberContext = memberContext;
        }

        private static readonly Dictionary<string, (bool?, bool?, bool?)> DescriptionMap = new Dictionary<string, (bool?, bool?, bool?)>
        {
            ["medlemskap"] = (true, null, null),
            ["medlemskap+kurs"] = (true, true, true),
            ["medlemskapgratisKurs"] = (true, true, true),
            ["medlemskap+trening"] = (true, true, null),
            ["2018"] = (false, false, false)
        };

        public async Task<int> SavePayments(IEnumerable<Charge> charges)
        {
            var count = 0;
            foreach(var charge in charges)
            {
                count += await SavePayment(charge) ? 1 : 0;
            }
            return count;
        }

        public async Task<bool> SavePayment(Charge charge)
        {
            if (!charge.Metadata.TryGetValue("email", out var email)) return false;
            if (!charge.Metadata.TryGetValue("name", out var name)) return false;

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
            return true;
        }

        private static Payment CreatePayment(Charge charge)
        {
            var (includesMembership, includesTraining, includesClasses) = GetIncludedFees(charge.Description);

            return new Payment
            {
                Amount = charge.Amount / 100m,
                Description = charge.Description,
                PayedAtUtc = charge.Created,
                StripeChargeId = charge.Id,
                IncludesMembership = charge.Metadata.TryGetValue("inc_membership", out var m) && m == "yes" || includesMembership,
                IncludesTraining = charge.Metadata.TryGetValue("inc_training", out var t) && t == "yes" || includesTraining,
                IncludesClasses = charge.Metadata.TryGetValue("inc_classes", out var c) && c == "yes" || includesClasses,
                Refunded = charge.Refunded
            };
        }

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
