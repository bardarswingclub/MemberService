namespace MemberService.Services;

public interface IVippsPaymentService
{
    Task<string> InitiatePayment(
        string userId,
        string description,
        decimal amount,
        string successUrl,
        string callbackUrl,
        bool includesMembership = false,
        bool includesTraining = false,
        bool includesClasses = false,
        Guid? eventId = null);

    Task CapturePayment(
        Guid orderId,
        string userId,
        string secret = null);
}