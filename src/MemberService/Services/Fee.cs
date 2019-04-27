namespace MemberService.Services
{
    public class Fee
    {
        public const string Membership = nameof(Membership);
        public const string Training = nameof(Training);
        public const string Classes = nameof(Classes);

        public Fee(
            string description,
            decimal amount,
            bool includesMembership = false,
            bool includesTraining = false,
            bool includesClasses = false)
        {
            Description = description;
            Amount = amount;
            IncludesMembership = includesMembership;
            IncludesTraining = includesTraining;
            IncludesClasses = includesClasses;
        }

        public string Description { get; }

        public decimal Amount { get; }

        public long AmountInCents => (long)Amount * 100;

        public bool IncludesMembership { get; }

        public bool IncludesTraining { get; }

        public bool IncludesClasses { get; }
    }
}