namespace MemberService.Configs;

public record Config
{
    public StripeConfig Stripe { get; init; }
    public VippsConfig Vipps { get; init; }
    public EmailConfig Email { get; init; }
    public string AdminEmails { get; init; }
    public Auth Authentication { get; init; }

    public record VippsConfig
    {
        public string BaseUrl { get; init; }
        public string SubscriptionKey { get; init; }
        public string MerchantSerialNumber { get; init; }
        public string CallbackPrefix { get; init; }
    }

    public record StripeConfig
    {
        public string PublicKey { get; init; }
        public string SecretKey { get; init; }
    }

    public class EmailConfig
    {
        public string From { get; init; }
        public string SendGridApiKey { get; init; }
    }

    public record Auth
    {
        public ClientOptions Microsoft { get; init; }
        public AppOptions Facebook { get; init; }
        public ClientOptions Vipps { get; init; }

        public record ClientOptions
        {
            public string ClientId { get; init; }
            public string ClientSecret { get; init; }
        }

        public record AppOptions
        {
            public string AppId { get; init; }
            public string AppSecret { get; init; }
        }
    }
}
