namespace MemberService.Services.Vipps.Models;
using System.Text.Json.Serialization;

public class Transaction
{
    [JsonPropertyName("amount")]
    public int Amount { get; init; }

    [JsonPropertyName("orderId")]
    public string OrderId { get; init; }

    [JsonPropertyName("timeStamp")]
    public DateTime TimeStamp { get; init; }

    [JsonPropertyName("transactionText")]
    public string TransactionText { get; init; }
}
