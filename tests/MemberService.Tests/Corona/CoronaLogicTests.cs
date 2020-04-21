using System;
using MemberService.Data;
using MemberService.Pages.Corona;
using MemberService.Services;
using Shouldly;

namespace MemberService.Tests.Corona
{
    using NUnit.Framework;

    [TestFixture]
    public class CoronaLogicTests
    {
        [Test]
        public void TestNoPayments()
        {
            var user = new User();

            user.CalculateCoronaRefund().ShouldBe(0m);
        }

        [Test]
        public void TestNoRelevantPayments()
        {
            var user = new User
            {
                Payments =
                {
                    new Payment
                    {
                        Amount = 500,
                        PayedAtUtc = DateTime.Now
                    }
                }
            };

            user.CalculateCoronaRefund().ShouldBe(0m);
        }

        [Test]
        public void TestOnlyMembership()
        {
            var user = new User();

            user.Payments.Add(Payment(user.GetMembershipFee()));

            user.CalculateCoronaRefund().ShouldBe(0m);
        }

        [Test]
        public void TestOnlyTraining()
        {
            var user = new User();

            user.Payments.Add(Payment(user.GetTrainingFee()));

            user.CalculateCoronaRefund().ShouldBe(FeeCalculation.TrainingFee / 2);
        }

        [Test]
        public void TestOnlyClasses()
        {
            var user = new User();

            user.Payments.Add(Payment(user.GetClassesFee()));

            user.CalculateCoronaRefund().ShouldBe(FeeCalculation.ClassesFee / 2);
        }

        [Test]
        public void TestOnlyTrainingLastYear()
        {
            var user = new User();

            user.Payments.Add(Payment(user.GetTrainingFee(), years: -1));

            user.CalculateCoronaRefund().ShouldBe(0);
        }

        [Test]
        public void TestOnlyClassesLastYear()
        {
            var user = new User();

            user.Payments.Add(Payment(user.GetClassesFee(), years: -1));

            user.CalculateCoronaRefund().ShouldBe(0);
        }

        [Test]
        public void TestTrainingAndMembership()
        {
            var user = new User();

            user.Payments.Add(Payment(user.GetMembershipFee()));
            user.Payments.Add(Payment(user.GetTrainingFee()));

            user.CalculateCoronaRefund().ShouldBe(FeeCalculation.TrainingFee / 2);
        }

        [Test]
        public void TestClassesAndTrainingAndMembership()
        {
            var user = new User();

            user.Payments.Add(Payment(user.GetMembershipFee()));
            user.Payments.Add(Payment(user.GetTrainingFee()));
            user.Payments.Add(Payment(user.GetClassesFee()));

            user.CalculateCoronaRefund().ShouldBe(FeeCalculation.ClassesFee / 2);
        }

        [Test]
        public void TestClassesAndTraining()
        {
            var user = new User();

            user.Payments.Add(Payment(user.GetTrainingFee()));
            user.Payments.Add(Payment(user.GetClassesFee()));

            user.CalculateCoronaRefund().ShouldBe(FeeCalculation.ClassesFee / 2);
        }

        private static Payment Payment((FeeStatus FeeStatus, Fee Fee) tuple, int years = 0) => new Payment
        {
            Amount = tuple.Fee.Amount,
            IncludesMembership = tuple.Fee.IncludesMembership,
            IncludesTraining = tuple.Fee.IncludesTraining,
            IncludesClasses = tuple.Fee.IncludesClasses,
            Description = tuple.Fee.Description,
            PayedAtUtc = DateTime.Now.AddYears(years)
        };
    }
}