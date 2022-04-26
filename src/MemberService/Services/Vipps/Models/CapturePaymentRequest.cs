namespace MemberService.Services.Vipps.Models;

public record CapturePaymentRequest
{
    public Merchant MerchantInfo { get; init; }

    public TransactionInfo Transaction { get; init; }

    public record Merchant(string MerchantSerialNumber);

    public record TransactionInfo(int Amount, string TransactionText);
}

public record CapturePaymentResponse
{
    public string PaymentInstrument { get; init; }
    public string OrderId { get; init; }
    public TransactionInfo TransactionInfo { get; init; }
    public TransactionSummary TransactionSummary { get; init; }
}
