namespace MemberService.Pages.Home
{
    public class StripePayment
    {
        public string stripeToken { get; set; }

        public string stripeEmail { get; set; }

        public long Amount { get; set; }
    }
}
