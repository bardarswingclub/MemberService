using System.Collections.Generic;
using System.Linq;
using MemberService.Data;

namespace MemberService.Pages.Corona
{
    public class CoronaModel
    {
        public IReadOnlyCollection<(Payment payment, decimal Amount)> Refund { get; set; }

        public decimal Sum => Refund.Sum(p => p.Amount);

        public bool IncludesClasses => Refund.Any(p => p.payment.IncludesClasses);

        public bool Authenticated { get; set; }
    }
}