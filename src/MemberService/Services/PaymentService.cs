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
            bool includesClasses = false,
            Guid? eventSignupId = null)
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
                    Description = title,
                    Metadata = new Dictionary<string, string>
                    {
                        ["name"] = name,
                        ["email"] = email,
                        ["amount_owed"] = amount.ToString(),
                        ["long_desc"] = description,
                        ["short_desc"] = title,
                        ["inc_membership"] = includesMembership ? "yes" : "no",
                        ["inc_training"] = includesTraining ? "yes" : "no",
                        ["inc_classes"] = includesClasses ? "yes" : "no",
                        ["event_signup"] = eventSignupId?.ToString()
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
                SuccessUrl = successUrl.Replace("%7BCHECKOUT_SESSION_ID%7D", "{CHECKOUT_SESSION_ID}"),
                CancelUrl = cancelUrl,
            });

            return session.Id;
        }

        public async Task<(int payments, int updates)> ImportPayments(string email)
        {
            var existingCustomers = await _customerService.ListAsync(new CustomerListOptions
            {
                Email = email
            });

            var paymentCreatedCount = 0;
            var paymentUpdatedCount = 0;
            foreach (var customer in existingCustomers)
            {
                var charges = await _chargeService.ListAsync(new ChargeListOptions
                {
                    CustomerId = customer.Id,
                    CreatedRange = new DateRangeOptions
                    {
                        GreaterThan = new DateTime(2019, 1, 1)
                    },
                });

                var (_, payments, updates) = await SavePayments(charges);
                paymentCreatedCount += payments;
                paymentUpdatedCount += updates;
            }

            return (paymentCreatedCount, paymentUpdatedCount);
        }

        public async Task<(int users, int payments, int updates)> SavePayment(string sessionId)
        {
            var session = await _sessionService.GetAsync(sessionId);

            var charges = await _chargeService.ListAsync(new ChargeListOptions
            {
                PaymentIntentId = session.PaymentIntentId
            });

            return await SavePayments(charges);
        }

        public async Task<(int users, int payments, int updates)> SavePayments(IEnumerable<Charge> charges)
        {
            var userCreatedCount = 0;
            var paymentCreatedCount = 0;
            var paymentUpdatedCount = 0;
            foreach (var charge in charges)
            {
                var result = await SavePayment(charge);
                userCreatedCount += result == Status.CreatedUser ? 1 : 0;
                paymentCreatedCount += result == Status.CreatedPayment ? 1 : 0;
                paymentUpdatedCount += result == Status.UpdatedPayment ? 1 : 0;
            }
            return (userCreatedCount, paymentCreatedCount, paymentUpdatedCount);
        }

        private async Task<Status> SavePayment(Charge charge)
        {
            var email = charge.Customer?.Email ?? charge.Metadata.GetValueOrDefault("email");
            var name = charge.Customer?.Name ?? charge.Metadata.GetValueOrDefault("name");

            if (email is null) return Status.Nothing;

            var user = await _memberContext.Users
                .Include(u => u.Payments)
                .Include(u => u.EventSignups)
                    .ThenInclude(s => s.AuditLog)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());

            if (user is null)
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

                await _memberContext.SaveChangesAsync();

                return Status.CreatedUser;
            }

            if (user.Payments.FirstOrDefault(p => p.StripeChargeId == charge.Id) is Payment payment)
            {
                payment.Refunded = charge.Refunded || charge.Status == "failed";
                payment.EventSignup = GetEventSignup(charge, user.EventSignups);

                SetEventSignupStatus(payment, user);

                var changes = await _memberContext.SaveChangesAsync();

                return changes == 0
                    ? Status.Nothing
                    : Status.UpdatedPayment;
            }

            payment = CreatePayment(charge, user.EventSignups);
            SetEventSignupStatus(payment, user);
            user.Payments.Add(payment);

            await _memberContext.SaveChangesAsync();

            return Status.CreatedPayment;
        }

        private static void SetEventSignupStatus(Payment payment, MemberUser user)
        {
            if (payment.EventSignup?.Status == Data.Status.Approved && !payment.Refunded)
            {
                payment.EventSignup.Status = Data.Status.AcceptedAndPayed;
                payment.EventSignup.AuditLog.Add("Paid", user, payment.PayedAtUtc);
            }
        }

        private static Payment CreatePayment(Charge charge, ICollection<EventSignup> eventSignups = null)
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
                Refunded = charge.Refunded || charge.Status == "failed",
                EventSignup = GetEventSignup(charge, eventSignups)
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

        private static EventSignup GetEventSignup(Charge charge, ICollection<EventSignup> eventSignups)
        {
            var id = charge.Metadata.GetValueOrDefault("event_signup")?.ToGuid();

            if (id == null)
                return null;

            return eventSignups?.FirstOrDefault(e => e.Id == id);
        }

        private enum Status
        {
            Nothing,
            CreatedUser,
            CreatedPayment,
            UpdatedPayment
        }
    }
}
