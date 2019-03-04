namespace MemberService.Services
{
    public class Fee
    {
        public Fee(string description, decimal amount)
        {
            Description = description;
            Amount = amount;
        }

        public string Description { get; set; }

        public decimal Amount { get; set; }

        public long AmountInCents => (long)Amount * 100;
    }
}