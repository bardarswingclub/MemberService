namespace MemberService.Services.Vipps.Models;
using System.Text.Json.Serialization;

public record MerchantInfo
{
    [JsonPropertyName("authToken")]
    public string AuthToken { get; init; }

    [JsonPropertyName("callbackPrefix")]
    public string CallbackPrefix { get; init; }

    [JsonPropertyName("consentRemovalPrefix")]
    public string ConsentRemovalPrefix { get; init; }

    [JsonPropertyName("fallBack")]
    public string FallBack { get; init; }

    [JsonPropertyName("isApp")]
    public bool IsApp { get; init; }

    [JsonPropertyName("merchantSerialNumber")]
    public string MerchantSerialNumber { get; init; }

    [JsonPropertyName("paymentType")]
    public string PaymentType { get; init; }

    [JsonPropertyName("shippingDetailsPrefix")]
    public string ShippingDetailsPrefix { get; init; }
}
