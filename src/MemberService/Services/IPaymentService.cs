using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public interface IPaymentService
    {
        Task<int> SavePayments(IEnumerable<Charge> charges);

        Task<bool> SavePayment(Charge charge);
    }
}
