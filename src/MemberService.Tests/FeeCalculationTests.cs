using NUnit.Framework;
using MemberService.Data;
using MemberService.Services;
using Shouldly;
using System.Collections.Generic;
using System;

namespace Tests
{
    [TestFixture]
    public class FeeCalculationTests
    {
        [Test]
        public void TestNoPayments()
        {
            var user = new MemberUser
            {
                Payments = new List<Payment>()
            };

            var fee = user.GetMembershipFee();

            fee.Description.ShouldBe("Medlemskap");
            fee.Amount.ShouldBe(300);
            fee.AmountInCents.ShouldBe(30000);
        }

        [Test]
        public void TestAlreadyPayed()
        {
            var user = new MemberUser
            {
                Payments = new[]
                {
                    new Payment
                    {
                        IncludesMembership = true,
                        PayedAt = DateTime.UtcNow
                    }
                }
            };

            var fee = user.GetMembershipFee();

            fee.ShouldBeNull();
        }

        [Test]
        public void TestPayedLastYear()
        {
            var user = new MemberUser
            {
                Payments = new[]
                {
                    new Payment
                    {
                        IncludesMembership = true,
                        PayedAt = DateTime.UtcNow.AddYears(-1)
                    }
                }
            };

            var fee = user.GetMembershipFee();

            fee.Description.ShouldBe("Medlemskap");
            fee.Amount.ShouldBe(300);
            fee.AmountInCents.ShouldBe(30000);
        }
    }
}