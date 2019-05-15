using Stripe;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public interface IPaymentService
    {
        Task<string> CreatePayment(
            string name,
            string email,
            string title,
            string description,
            decimal amount,
            string successUrl,
            string cancelUrl,
            bool includesMembership = false,
            bool includesTraining = false,
            bool includesClasses = false);

        Task<int> SavePayments(IEnumerable<Charge> charges);

        Task<int> SavePayment(string sessionId);
    }
}
