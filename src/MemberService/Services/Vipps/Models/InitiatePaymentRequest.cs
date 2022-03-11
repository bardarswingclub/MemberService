namespace MemberService.Services.Vipps.Models;

public record InitiatePaymentRequest
{
    public CustomerInfo CustomerInfo { get; init; }
    public MerchantInfo MerchantInfo { get; init; }
    public Transaction Transaction { get; init; }
}
