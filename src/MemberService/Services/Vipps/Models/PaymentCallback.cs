namespace MemberService.Services.Vipps.Models;

public record PaymentCallback
{
    public string MerchantSerialNumber { get; init; }
    public string OrderId { get; init; }
    public TransactionInfo TransactionInfo { get; init; }
}
