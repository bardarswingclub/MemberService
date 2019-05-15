using MemberService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly SessionService _sessionService;
        private readonly ChargeService _chargeService;
        private readonly CustomerService _customerService;
        private readonly UserManager<MemberUser> _userManager;
        private readonly MemberContext _memberContext;

        public PaymentService(
            SessionService sessionService,
            ChargeService chargeService,
            CustomerService customerService,
            UserManager<MemberUser> userManager,
            MemberContext memberContext)
        {
            _sessionService = sessionService;
            _chargeService = chargeService;
            _customerService = customerService;
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

        public async Task<string> CreatePayment(
            string name,
            string email,
            string title,
            string description,
            decimal amount,
            string successUrl,
            string cancelUrl,
            bool includesMembership = false,
            bool includesTraining = false,
            bool includesClasses = false)
        {
            var existingCustomers = await _customerService.ListAsync(new CustomerListOptions
            {
                Email = email,
                Limit = 1
            });

            var customer = existingCustomers.FirstOrDefault()
                ?? await _customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = email,
                    Name = name
                });

            var session = await _sessionService.CreateAsync(new SessionCreateOptions
            {
                CustomerId = customer.Id,
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Description = $"{title} ({description})",
                    Metadata = new Dictionary<string, string>
                    {
                        ["name"] = name,
                        ["email"] = email,
                        ["amount_owed"] = amount.ToString(),
                        ["long_desc"] = description,
                        ["short_desc"] = title,
                        ["inc_membership"] = includesMembership ? "yes" : "no",
                        ["inc_training"] = includesTraining ? "yes" : "no",
                        ["inc_classes"] = includesClasses ? "yes" : "no"
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
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            });

            return session.Id;
        }

        public async Task<int> SavePayment(string sessionId)
        {
            var session = await _sessionService.GetAsync(sessionId);

            var charges = await _chargeService.ListAsync(new ChargeListOptions
            {
                PaymentIntentId = session.PaymentIntentId
            });

            return await SavePayments(charges);
        }

        public async Task<int> SavePayments(IEnumerable<Charge> charges)
        {
            var count = 0;
            foreach(var charge in charges)
            {
                count += await SavePayment(charge) ? 1 : 0;
            }
            return count;
        }

        private async Task<bool> SavePayment(Charge charge)
        {
            var email = charge.Customer?.Email ?? charge.Metadata.GetValueOrDefault("email");
            var name = charge.Customer?.Name ?? charge.Metadata.GetValueOrDefault("name");

            if (email is null) return false;

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
