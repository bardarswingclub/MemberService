namespace MemberService.Services.Vipps;
using System.Threading.Tasks;

using MemberService.Services.Vipps.Models;

public interface IVippsClient
{
    Task<InitiatePaymentResponse> InitiatePayment(Transaction transaction, string returnUrl, string callbackUrl);

    Task<CapturePaymentResponse> CapturePayment(string orderId, string transactionText);

    Task<PaymentDetails> GetPaymentDetails(string orderId);
}
