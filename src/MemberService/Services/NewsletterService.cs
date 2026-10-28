namespace MemberService.Services;

using Microsoft.Extensions.Options;

public interface INewsletterService
{
    /// <summary>Subscribes the address to the newsletter. Never throws; returns false if it failed.</summary>
    Task<bool> Subscribe(string email, string fullName);
}

public class NewsletterSettings
{
    /// <summary>The "action" of the Mailchimp embedded signup form.</summary>
    public string SubscribeUrl { get; set; }

    /// <summary>Name of the hidden bot-trap field in the Mailchimp form. Must be sent empty.</summary>
    public string HoneypotField { get; set; }
}

public class MailchimpNewsletterService : INewsletterService
{
    private readonly HttpClient _httpClient;
    private readonly NewsletterSettings _settings;
    private readonly ILogger<MailchimpNewsletterService> _logger;

    public MailchimpNewsletterService(
        HttpClient httpClient,
        IOptions<NewsletterSettings> options,
        ILogger<MailchimpNewsletterService> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<bool> Subscribe(string email, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(_settings.SubscribeUrl))
        {
            return false;
        }

        var (firstName, lastName) = SplitName(fullName);

        var fields = new Dictionary<string, string>
        {
            ["EMAIL"] = email,
            ["FNAME"] = firstName,
            ["LNAME"] = lastName
        };

        if (!string.IsNullOrEmpty(_settings.HoneypotField))
        {
            fields[_settings.HoneypotField] = "";
        }

        try
        {
            using var response = await _httpClient.PostAsync(_settings.SubscribeUrl, new FormUrlEncodedContent(fields));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Newsletter signup failed with status {StatusCode}", response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            // Newsletter signup is a nice-to-have and must never get in the way of joining the club
            _logger.LogWarning(e, "Newsletter signup failed");
            return false;
        }
    }

    public static (string FirstName, string LastName) SplitName(string fullName)
    {
        var parts = (fullName ?? "").Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length switch
        {
            0 => ("", ""),
            1 => (parts[0], ""),
            _ => (parts[0], parts[1])
        };
    }
}
