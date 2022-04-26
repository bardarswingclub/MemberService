namespace MemberService.Services.Vipps.Models;

public record TransactionSummary
{
    public int CapturedAmount { get; init; }
    public int RefundedAmount { get; init; }
    public int RemainingAmountToCapture { get; init; }
    public int RemainingAmountToRefund { get; init; }
    public int BankIdentificationNumber { get; init; }
}