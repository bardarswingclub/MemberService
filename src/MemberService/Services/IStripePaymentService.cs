namespace MemberService.Services;

using Stripe;

public interface IStripePaymentService
{
    Task<string> CreatePaymentRequest(
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
        Guid? eventSignupId = null);

    Task<(int users, int payments, int updates)> SavePayments(IEnumerable<Charge> charges);

    Task<(int users, int payments, int updates)> SavePayment(string sessionId);

    Task<(int payments, int updates)> ImportPayments(string email);

    Task<bool> Refund(string paymentId);
}
