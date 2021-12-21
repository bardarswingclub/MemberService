namespace MemberService.Pages.Corona;




using MemberService.Data;

public class CoronaModel
{
    public IReadOnlyCollection<(Payment payment, decimal Amount)> Refund { get; set; }

    public decimal Sum => Refund.Sum(p => p.Amount);

    public bool IncludesClasses => Refund.Any(p => p.payment.IncludesClasses);

    public bool Authenticated { get; set; }
}
