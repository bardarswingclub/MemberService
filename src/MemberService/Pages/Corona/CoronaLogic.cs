using System.Collections.Generic;
using System.Linq;
using Clave.ExtensionMethods;
using MemberService.Data;
using MemberService.Services;

namespace MemberService.Pages.Corona
{
    public static class CoronaLogic
    {
        public static decimal CalculateCoronaRefund(this User user)
        {
            var payments = user.GetCoronaRefundablePayments();

            return payments.Sum(p => p.Amount);
        }

        public static IEnumerable<(Payment payment, decimal Amount)> GetCoronaRefundablePayments(this User user)
        {
            var paymentsThisSemester = user.Payments
                .Where(p => p.PayedAtUtc > TimeProvider.ThisSemesterUtc && p.PayedAtUtc < TimeProvider.NextSemesterUtc)
                .Where(p => p.IncludesClasses || p.IncludesTraining)
                .WhereNot(p => p.Refunded);

            return paymentsThisSemester.Select(p =>
            {
                var amount = p.Amount;
                if (p.IncludesMembership)
                {
                    amount -= FeeCalculation.MembershipFee;
                }

                return (Payment: p, Amount: amount / 2);
            });
        }
    }
}