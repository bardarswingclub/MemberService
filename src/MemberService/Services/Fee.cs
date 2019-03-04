namespace MemberService.Services
{
    public class Fee
    {
        public const string Membership = nameof(Membership);
        public const string Training = nameof(Training);
        public const string Classes = nameof(Classes);

        public Fee(string description, decimal amount)
        {
            Description = description;
            Amount = amount;
        }

        public string Description { get; }

        public decimal Amount { get; }

        public long AmountInCents => (long)Amount * 100;

        public bool IncludesMembership { get; set; }

        public bool IncludesTraining { get; set; }

        public bool IncludesClasses { get; set; }
    }
}