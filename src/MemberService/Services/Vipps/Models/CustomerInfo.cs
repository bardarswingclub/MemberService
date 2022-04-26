namespace MemberService.Services.Vipps.Models;
using System.Text.Json.Serialization;

public record CustomerInfo
{
    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; init; }
}
