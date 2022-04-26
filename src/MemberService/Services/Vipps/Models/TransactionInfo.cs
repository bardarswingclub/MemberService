namespace MemberService.Services.Vipps.Models;

public record TransactionInfo
{
    public int Amount { get; init; }
    public string Status { get; init; }
    public DateTime TimeStamp { get; init; }
    public string TransactionId { get; init; }
    public string TransactionText { get; init; }
}
