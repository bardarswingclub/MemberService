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
        public void TestMembershipFee_NoPayments()
        {
            var user = new MemberUser
            {
                Payments = new List<Payment>()
            };

            var fee = user.GetMembershipFee();

            fee.Description.ShouldBe("Medlemskap");
            fee.Amount.ShouldBe(300);
            fee.AmountInCents.ShouldBe(300_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeFalse();
            fee.IncludesClasses.ShouldBeFalse();
        }

        [Test]
        public void TestMembershipFee_AlreadyPayed()
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
        public void TestMembershipFee_PayedLastYear()
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
            fee.AmountInCents.ShouldBe(300_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeFalse();
            fee.IncludesClasses.ShouldBeFalse();
        }

        [Test]
        public void TestTrainingFee_NoPayments()
        {
            var user = new MemberUser
            {
                Payments = new List<Payment>()
            };

            var fee = user.GetTrainingFee();

            fee.Description.ShouldBe("Medlemskap og treningsavgift");
            fee.Amount.ShouldBe(725);
            fee.AmountInCents.ShouldBe(725_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeTrue();
            fee.IncludesClasses.ShouldBeFalse();
        }

        [Test]
        public void TestTrainingFee_AlreadyPayed()
        {
            var user = new MemberUser
            {
                Payments = new[]
                {
                    new Payment
                    {
                        IncludesMembership = true,
                        IncludesTraining = true,
                        PayedAt = DateTime.UtcNow
                    }
                }
            };

            var fee = user.GetTrainingFee();

            fee.ShouldBeNull();
        }

        [Test]
        public void TestTrainingFee_PayedLastYear()
        {
            var user = new MemberUser
            {
                Payments = new[]
                {
                    new Payment
                    {
                        IncludesMembership = true,
                        IncludesTraining = true,
                        PayedAt = DateTime.UtcNow.AddYears(-1)
                    }
                }
            };

            var fee = user.GetTrainingFee();

            fee.Description.ShouldBe("Medlemskap og treningsavgift");
            fee.Amount.ShouldBe(725);
            fee.AmountInCents.ShouldBe(725_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeTrue();
            fee.IncludesClasses.ShouldBeFalse();
        }

        [Test]
        public void TestTrainingFee_MembershipPayed()
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

            var fee = user.GetTrainingFee();

            fee.Description.ShouldBe("Treningsavgift");
            fee.Amount.ShouldBe(425);
            fee.AmountInCents.ShouldBe(425_00);
            fee.IncludesMembership.ShouldBeFalse();
            fee.IncludesTraining.ShouldBeTrue();
            fee.IncludesClasses.ShouldBeFalse();
        }

        [Test]
        public void TestClassesFee_NoPayments()
        {
            var user = new MemberUser
            {
                Payments = new List<Payment>()
            };

            var fee = user.GetClassesFee();

            fee.Description.ShouldBe("Medlemskap og kursavgift");
            fee.Amount.ShouldBe(1200);
            fee.AmountInCents.ShouldBe(1200_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeTrue();
            fee.IncludesClasses.ShouldBeTrue();
        }

        [Test]
        public void TestClassesFee_AlreadyPayed()
        {
            var user = new MemberUser
            {
                Payments = new[]
                {
                    new Payment
                    {
                        IncludesMembership = true,
                        IncludesTraining = true,
                        IncludesClasses = true,
                        PayedAt = DateTime.UtcNow
                    }
                }
            };

            var fee = user.GetClassesFee();

            fee.ShouldBeNull();
        }

        [Test]
        public void TestClassesFee_PayedLastYear()
        {
            var user = new MemberUser
            {
                Payments = new[]
                {
                    new Payment
                    {
                        IncludesMembership = true,
                        IncludesTraining = true,
                        IncludesClasses = true,
                        PayedAt = DateTime.UtcNow.AddYears(-1)
                    }
                }
            };

            var fee = user.GetClassesFee();

            fee.Description.ShouldBe("Medlemskap og kursavgift");
            fee.Amount.ShouldBe(1200);
            fee.AmountInCents.ShouldBe(1200_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeTrue();
            fee.IncludesClasses.ShouldBeTrue();
        }

        [Test]
        public void TestClassesFee_MembershipPayed()
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

            var fee = user.GetClassesFee();

            fee.Description.ShouldBe("Kursavgift");
            fee.Amount.ShouldBe(900);
            fee.AmountInCents.ShouldBe(900_00);
            fee.IncludesMembership.ShouldBeFalse();
            fee.IncludesTraining.ShouldBeTrue();
            fee.IncludesClasses.ShouldBeTrue();
        }

        [Test]
        public void TestClassesFee_TrainingPayed()
        {
            var user = new MemberUser
            {
                Payments = new[]
                {
                    new Payment
                    {
                        IncludesMembership = true,
                        IncludesTraining = true,
                        PayedAt = DateTime.UtcNow
                    }
                }
            };

            var fee = user.GetClassesFee();

            fee.Description.ShouldBe("Kursavgift");
            fee.Amount.ShouldBe(475);
            fee.AmountInCents.ShouldBe(475_00);
            fee.IncludesMembership.ShouldBeFalse();
            fee.IncludesTraining.ShouldBeFalse();
            fee.IncludesClasses.ShouldBeTrue();
        }
    }
}