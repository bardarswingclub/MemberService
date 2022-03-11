namespace MemberService.Services.Vipps.Models;

public record Transaction
{
    public int Amount { get; init; }
    public string OrderId { get; init; }
    public string TransactionText { get; init; }
}
