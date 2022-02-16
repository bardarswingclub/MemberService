namespace MemberService.Configs;

public record Config
{
    public StripeConfig Stripe { get; init; }
    public EmailConfig Email { get; init; }
    public string AdminEmails { get; init; }
    public Auth Authentication { get; init; }

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
        public MicrosoftOptions Microsoft { get; init; }
        public FacebookOptions Facebook { get; init; }

        public record MicrosoftOptions
        {
            public string ClientId { get; init; }
            public string ClientSecret { get; init; }
        }

        public record FacebookOptions
        {
            public string AppId { get; init; }
            public string AppSecret { get; init; }
        }
    }
}
