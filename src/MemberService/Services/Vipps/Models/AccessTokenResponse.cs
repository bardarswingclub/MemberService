namespace MemberService.Services.Vipps.Models;
using System;
using System.Text.Json.Serialization;

public record AccessTokenResponse
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("ext_expires_in")]
    public int ExtExpiresIn { get; init; }

    [JsonPropertyName("expires_on")]
    public int ExpiresOn { get; init; }

    [JsonPropertyName("not_before")]
    public int NotBefore { get; init; }

    [JsonPropertyName("resource")]
    public string Resources { get; init; }

    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    public bool HasExpired => DateTimeOffset.UtcNow.AddSeconds(30) > DateTimeOffset.FromUnixTimeSeconds(ExpiresOn);
}
