namespace MemberService.Configs
{
    public class Config
    {
        public StripeConfig Stripe { get; set; }

        public EmailConfig Email { get; set; }

        public string AdminEmails { get; set; }
    }
}