namespace MemberService.Services.Vipps.Models;

public record InitiatePaymentResponse
{
    public string OrderId { get; init; }
    public string Url { get; init; }
}