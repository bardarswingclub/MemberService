using NUnit.Framework;
using MemberService.Data;
using MemberService.Services;
using Shouldly;
using System;

namespace MemberService.Tests
{
    [TestFixture]
    public class FeeCalculationTests
    {
        [Test]
        public void TestMembershipFee_NoPayments()
        {
            var user = new User();

            var (status, fee) = user.GetMembershipFee();

            status.ShouldBe(FeeStatus.Unpaid);

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
            var user = new User
            {
                Payments =
                {
                    MembershipPayment()
                }
            };

            var (status, fee) = user.GetMembershipFee();

            status.ShouldBe(FeeStatus.Paid);

            fee.ShouldBeNull();
        }

        [Test]
        public void TestMembershipFee_PayedLastYear()
        {
            var user = new User
            {
                Payments =
                {
                    MembershipPayment(-1)
                }
            };

            var (status, fee) = user.GetMembershipFee();

            status.ShouldBe(FeeStatus.Unpaid);

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
            var user = new User();

            var (status, fee) = user.GetTrainingFee();

            status.ShouldBe(FeeStatus.Unpaid);

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
            var user = new User
            {
                Payments =
                {
                    MembershipPayment(),
                    TrainingPayment()
                }
            };

            var (status, fee) = user.GetTrainingFee();

            status.ShouldBe(FeeStatus.Paid);

            fee.ShouldBeNull();
        }

        [Test]
        public void TestTrainingFee_PayedLastYear()
        {
            var user = new User
            {
                Payments =
                {
                    MembershipPayment(-1),
                    TrainingPayment(-1)
                }
            };

            var (status, fee) = user.GetTrainingFee();

            status.ShouldBe(FeeStatus.Unpaid);

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
            var user = new User
            {
                Payments =
                {
                    MembershipPayment()
                }
            };

            var (status, fee) = user.GetTrainingFee();

            status.ShouldBe(FeeStatus.Unpaid);

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
            var user = new User();

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Unpaid);

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
            var user = new User
            {
                Payments =
                {
                    MembershipPayment(),
                    TrainingPayment(),
                    ClassesPayment()
                }
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Paid);

            fee.ShouldBeNull();
        }

        [Test]
        public void TestClassesFee_PayedLastYear()
        {
            var user = new User
            {
                Payments =
                {
                    MembershipPayment(-1),
                    TrainingPayment(-1),
                    ClassesPayment(-1)
                }
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Unpaid);

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
            var user = new User
            {
                Payments =
                {
                    MembershipPayment()
                }
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Unpaid);

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
            var user = new User
            {
                Payments =
                {
                    MembershipPayment(),
                    TrainingPayment()
                }
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Unpaid);

            fee.Description.ShouldBe("Kursavgift");
            fee.Amount.ShouldBe(475);
            fee.AmountInCents.ShouldBe(475_00);
            fee.IncludesMembership.ShouldBeFalse();
            fee.IncludesTraining.ShouldBeFalse();
            fee.IncludesClasses.ShouldBeTrue();
        }

        [Test]
        public void TestMembershipFee_ExcemptTraining_NoPayments()
        {
            var user = new User
            {
                ExemptFromTrainingFee = true
            };

            var (status, fee) = user.GetMembershipFee();

            status.ShouldBe(FeeStatus.Unpaid);

            fee.Description.ShouldBe("Medlemskap");
            fee.Amount.ShouldBe(300);
            fee.AmountInCents.ShouldBe(300_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeFalse();
            fee.IncludesClasses.ShouldBeFalse();
        }

        [Test]
        public void TestTrainingFee_ExcemptTraining_NoPayments()
        {
            var user = new User
            {
                ExemptFromTrainingFee = true
            };

            var (status, fee) = user.GetTrainingFee();

            status.ShouldBe(FeeStatus.Unpayable);

            fee.ShouldBeNull();
        }

        [Test]
        public void TestClassesFee_ExcemptTraining_NoPayments()
        {
            var user = new User
            {
                ExemptFromTrainingFee = true
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Unpaid);

            fee.Description.ShouldBe("Medlemskap og kursavgift");
            fee.Amount.ShouldBe(775);
            fee.AmountInCents.ShouldBe(775_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeFalse();
            fee.IncludesClasses.ShouldBeTrue();
        }

        [Test]
        public void TestClassesFee_ExcemptTraining_MembershipPayed()
        {
            var user = new User
            {
                ExemptFromTrainingFee = true,
                Payments =
                {
                    MembershipPayment()
                }
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Unpaid);

            fee.Description.ShouldBe("Kursavgift");
            fee.Amount.ShouldBe(475);
            fee.AmountInCents.ShouldBe(475_00);
            fee.IncludesMembership.ShouldBeFalse();
            fee.IncludesTraining.ShouldBeFalse();
            fee.IncludesClasses.ShouldBeTrue();
        }

        [Test]
        public void TestMembershipFee_ExcemptTrainingAndClasses_NoPayments()
        {
            var user = new User
            {
                ExemptFromTrainingFee = true,
                ExemptFromClassesFee = true
            };

            var (status, fee) = user.GetMembershipFee();

            status.ShouldBe(FeeStatus.Unpaid);

            fee.Description.ShouldBe("Medlemskap");
            fee.Amount.ShouldBe(300);
            fee.AmountInCents.ShouldBe(300_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeFalse();
            fee.IncludesClasses.ShouldBeFalse();
        }

        [Test]
        public void TestTrainingFee_ExcemptTrainingAndClasses_NoPayments()
        {
            var user = new User
            {
                ExemptFromTrainingFee = true,
                ExemptFromClassesFee = true
            };

            var (status, fee) = user.GetTrainingFee();

            status.ShouldBe(FeeStatus.Unpayable);

            fee.ShouldBeNull();
        }

        [Test]
        public void TestClassesFee_ExcemptTrainingAndClasses_NoPayments()
        {
            var user = new User
            {
                ExemptFromTrainingFee = true,
                ExemptFromClassesFee = true
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Unpayable);

            fee.ShouldBeNull();
        }

        [Test]
        public void TestClassesFee_ExcemptTrainingAndClasses_MembershipPayed()
        {
            var user = new User
            {
                ExemptFromTrainingFee = true,
                ExemptFromClassesFee = true,
                Payments =
                {
                    MembershipPayment()
                }
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Paid);

            fee.ShouldBeNull();
        }

        [Test]
        public void TestMembershipFee_ExcemptClasses_NoPayments()
        {
            var user = new User
            {
                ExemptFromClassesFee = true
            };

            var (status, fee) = user.GetMembershipFee();

            status.ShouldBe(FeeStatus.Unpaid);

            fee.Description.ShouldBe("Medlemskap");
            fee.Amount.ShouldBe(300);
            fee.AmountInCents.ShouldBe(300_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeFalse();
            fee.IncludesClasses.ShouldBeFalse();
        }

        [Test]
        public void TestTrainingFee_ExcemptClasses_NoPayments()
        {
            var user = new User
            {
                ExemptFromClassesFee = true
            };

            var (status, fee) = user.GetTrainingFee();

            status.ShouldBe(FeeStatus.Unpaid);

            fee.Description.ShouldBe("Medlemskap og treningsavgift");
            fee.Amount.ShouldBe(725);
            fee.AmountInCents.ShouldBe(725_00);
            fee.IncludesMembership.ShouldBeTrue();
            fee.IncludesTraining.ShouldBeTrue();
            fee.IncludesClasses.ShouldBeFalse();
        }

        [Test]
        public void TestClassesFee_ExcemptClasses_NoPayments()
        {
            var user = new User
            {
                ExemptFromClassesFee = true
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Unpayable);

            fee.ShouldBeNull();
        }

        [Test]
        public void TestClassesFee_ExcemptClasses_MembershipPayed()
        {
            var user = new User
            {
                ExemptFromClassesFee = true,
                Payments =
                {
                    MembershipPayment()
                }
            };

            var (status, fee) = user.GetClassesFee();

            status.ShouldBe(FeeStatus.Unpayable);

            fee.ShouldBeNull();
        }

        private static Payment MembershipPayment(int years = 0) => new Payment
        {
            IncludesMembership = true,
            PayedAtUtc = DateTime.UtcNow.AddYears(years)
        };

        private static Payment TrainingPayment(int years = 0) => new Payment
        {
            IncludesTraining = true,
            PayedAtUtc = DateTime.UtcNow.AddYears(years)
        };

        private static Payment ClassesPayment(int years = 0) => new Payment
        {
            IncludesClasses = true,
            PayedAtUtc = DateTime.UtcNow.AddYears(years)
        };
    }
}