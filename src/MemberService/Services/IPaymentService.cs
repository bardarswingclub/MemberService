using Stripe;
using System;
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
            bool includesClasses = false,
            Guid? eventSignupId = null);

        Task<(int users, int payments, int updates)> SavePayments(IEnumerable<Charge> charges);

        Task<(int users, int payments, int updates)> SavePayment(string sessionId);
    }
}
