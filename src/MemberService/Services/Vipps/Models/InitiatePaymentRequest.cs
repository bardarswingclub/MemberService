namespace MemberService.Services.Vipps.Models;
using System.Text.Json.Serialization;

public record InitiatePaymentRequest
{
    [JsonPropertyName("customerInfo")]
    public CustomerInfo CustomerInfo { get; init; }

    [JsonPropertyName("merchantInfo")]
    public MerchantInfo MerchantInfo { get; init; }

    [JsonPropertyName("transaction")]
    public Transaction Transaction { get; init; }
}

public record InitiatePaymentResponse
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}