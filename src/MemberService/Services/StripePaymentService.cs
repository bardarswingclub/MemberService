namespace MemberService.Services;

using MemberService.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Stripe;
using Stripe.Checkout;

public class StripePaymentService : IStripePaymentService
{
    private const string Membership = "medlemskap";
    private const string MembershipAndCourse = "medlemskap+kurs";
    private const string MembershipFreeCourse = "medlemskapgratisKurs";
    private const string MembershipTraining = "medlemskap+trening";
    private const string Old = "2018";
    private const string Name = "name";
    private const string Email = "email";
    private const string AmountOwed = "amount_owed";
    private const string LongDescription = "long_desc";
    private const string ShortDescription = "short_desc";
    private const string IncludesMembership = "inc_membership";
    private const string IncludesTraining = "inc_training";
    private const string IncludesClasses = "inc_classes";
    private const string EventSignup = "event_signup";
    private const string Event = "event";

    private readonly SessionService _sessionService;
    private readonly ChargeService _chargeService;
    private readonly CustomerService _customerService;
    private readonly RefundService _refundService;
    private readonly UserManager<User> _userManager;
    private readonly MemberContext _memberContext;

    public StripePaymentService(
        SessionService sessionService,
        ChargeService chargeService,
        CustomerService customerService,
        UserManager<User> userManager,
        MemberContext memberContext,
        RefundService refundService)
    {
        _sessionService = sessionService;
        _chargeService = chargeService;
        _customerService = customerService;
        _userManager = userManager;
        _memberContext = memberContext;
        _refundService = refundService;
    }

    private static readonly Dictionary<string, (bool?, bool?, bool?)> DescriptionMap = new()
    {
        [Membership] = (true, null, null),
        [MembershipAndCourse] = (true, true, true),
        [MembershipFreeCourse] = (true, true, true),
        [MembershipTraining] = (true, true, null),
        [Old] = (false, false, false)
    };

    public async Task<string> CreatePaymentRequest(
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
        Guid? eventId = null)
    {
        var customerId = await GetCustomerId(email, name);

        try
        {
            var session = await _sessionService.CreateAsync(new SessionCreateOptions
            {
                Customer = customerId,
                PaymentIntentData = new()
                {
                    Description = title,
                    Metadata = new()
                    {
                        [Name] = name,
                        [Email] = email,
                        [AmountOwed] = amount.ToString(),
                        [LongDescription] = description,
                        [ShortDescription] = title,
                        [IncludesMembership] = includesMembership ? "yes" : "no",
                        [IncludesTraining] = includesTraining ? "yes" : "no",
                        [IncludesClasses] = includesClasses ? "yes" : "no",
                        [Event] = eventId?.ToString()
                    }
                },
                PaymentMethodTypes = new() { "card" },
                LineItems = new()
                {
                    new()
                    {
                        Name = title,
                        Description = description,
                        Amount = (long)amount * 100L,
                        Currency = "nok",
                        Quantity = 1
                    }
                },
                SuccessUrl = successUrl.Replace("%7BCHECKOUT_SESSION_ID%7D", "{CHECKOUT_SESSION_ID}"),
                CancelUrl = cancelUrl,
            });

            return session.Url;
        }
        catch(StripeException ex) when (ex.Message == "Not a valid URL")
        {
            throw new Exception($"Not a valid URL: {successUrl}, {cancelUrl}", ex);
        }
    }

    private async Task<string> GetCustomerId(string email, string name)
    {
        if (email == null || name == null)
        {
            return null;
        }

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

        return customer.Id;
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
                Customer = customer.Id,
                Created = new DateRangeOptions
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
            PaymentIntent = session.PaymentIntentId
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

    public async Task<bool> Refund(string paymentId)
    {
        var payment = await _memberContext.Payments.FindAsync(paymentId);

        await _refundService.CreateAsync(new RefundCreateOptions
        {
            Amount = (long?)(payment.Amount * 100),
            Charge = payment.StripeChargeId,
            Reason = "requested_by_customer",
            Metadata = new Dictionary<string, string>
            {
                ["Description"] = "Refunded by customer request"
            }
        });

        payment.Refunded = true;

        await _memberContext.SaveChangesAsync();

        return true;
    }

    private async Task<Status> SavePayment(Charge charge)
    {
        var email = charge.Customer?.Email ?? charge.Metadata.GetValueOrDefault(Email);
        var name = charge.Customer?.Name ?? charge.Metadata.GetValueOrDefault(Name);

        if (email is null) return Status.Nothing;

        var user = await _memberContext.Users
            .Include(u => u.Payments.Where(p => p.StripeChargeId == charge.Id))
            .Include(u => u.EventSignups)
                .ThenInclude(s => s.AuditLog)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant());

        if (user is null)
        {
            await _userManager.CreateAsync(new User
            {
                UserName = email,
                Email = email,
                FullName = name,
                Payments =
                {
                    CreatePayment(charge)
                }
            });

            await _memberContext.SaveChangesAsync();

            return Status.CreatedUser;
        }
        else if (user.Payments.FirstOrDefault(p => p.StripeChargeId == charge.Id) is Payment payment)
        {
            payment.Refunded = charge.Refunded || charge.Status == "failed";
            payment.EventSignup = GetEventSignup(charge, user.EventSignups);
            payment.IncludesMembership = charge.Metadata.TryGetValue(IncludesMembership, out var m) && m == "yes";
            payment.IncludesTraining = charge.Metadata.TryGetValue(IncludesTraining, out var t) && t == "yes";
            payment.IncludesClasses = charge.Metadata.TryGetValue(IncludesClasses, out var c) && c == "yes";

            SetEventSignupStatus(payment, user);

            var changes = await _memberContext.SaveChangesAsync();

            return changes == 0
                ? Status.Nothing
                : Status.UpdatedPayment;
        }
        else
        {
            payment = CreatePayment(charge, user.EventSignups);
            SetEventSignupStatus(payment, user);
            user.Payments.Add(payment);

            await _memberContext.SaveChangesAsync();

            return Status.CreatedPayment;
        }
    }

    private static void SetEventSignupStatus(Payment payment, User user)
    {
        if (payment.EventSignup?.Status == Data.ValueTypes.Status.Approved && !payment.Refunded)
        {
            payment.EventSignup.Status = Data.ValueTypes.Status.AcceptedAndPayed;
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
            IncludesMembership = charge.Metadata.TryGetValue(IncludesMembership, out var m) && m == "yes" || includesMembership,
            IncludesTraining = charge.Metadata.TryGetValue(IncludesTraining, out var t) && t == "yes" || includesTraining,
            IncludesClasses = charge.Metadata.TryGetValue(IncludesClasses, out var c) && c == "yes" || includesClasses,
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
        if (charge.Metadata.GetValueOrDefault(EventSignup)?.ToGuid() is Guid eventSignupId)
            return eventSignups?.FirstOrDefault(e => e.Id == eventSignupId);

        if (charge.Metadata.GetValueOrDefault(Event)?.ToGuid() is Guid eventId)
            return eventSignups?.FirstOrDefault(e => e.EventId == eventId);

        return null;
    }

    private enum Status
    {
        Nothing,
        CreatedUser,
        CreatedPayment,
        UpdatedPayment
    }
}
