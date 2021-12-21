namespace MemberService.Tests;

using MemberService.Data;

using NUnit.Framework;

using Shouldly;

[TestFixture]
public class UserTests
{
    [Test]
    public void TestNoPayments()
    {
        var user = new User();

        user.HasPayedMembershipThisYear().ShouldBeFalse();
        user.HasPayedTrainingFeeThisSemester().ShouldBeFalse();
        user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
    }

    [Test]
    public void TestPaidLastYear()
    {
        var user = new User
        {
            Payments =
                {
                    Payment(paidAt: TimeProvider.UtcNow.AddYears(-1), membership: true, training: true, classes: true)
                }
        };

        user.HasPayedMembershipThisYear().ShouldBeFalse();
        user.HasPayedTrainingFeeThisSemester().ShouldBeFalse();
        user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
    }

    [Test]
    public void TestPaidButRefunded()
    {
        using (TemporaryTime.Is(new DateTime(2019, 10, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-3), membership: true, training: true, classes: true, refunded: true)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeFalse();
            user.HasPayedTrainingFeeThisSemester().ShouldBeFalse();
            user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
        }
    }

    [Test]
    public void TestPaidLastSemester()
    {
        using (TemporaryTime.Is(new DateTime(2019, 10, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-6), membership: true, training: true, classes: true)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeFalse();
            user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
        }
    }

    [Test]
    public void TestPaidAllThisFallSemester()
    {
        using (TemporaryTime.Is(new DateTime(2019, 10, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-1), membership: true, training: true, classes: true)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeTrue();
            user.HasPayedClassesFeeThisSemester().ShouldBeTrue();
        }
    }

    [Test]
    public void TestPaidMembershipThisFallSemester()
    {
        using (TemporaryTime.Is(new DateTime(2019, 10, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-1), membership: true, training: false, classes: false)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeFalse();
            user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
        }
    }

    [Test]
    public void TestPaidTrainingThisFallSemester()
    {
        using (TemporaryTime.Is(new DateTime(2019, 10, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-1), membership: true, training: true, classes: false)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeTrue();
            user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
        }
    }

    [Test]
    public void TestPaidAllThisSpringSemester()
    {
        using (TemporaryTime.Is(new DateTime(2019, 3, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-1), membership: true, training: true, classes: true)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeTrue();
            user.HasPayedClassesFeeThisSemester().ShouldBeTrue();
        }
    }

    [Test]
    public void TestPaidMembershipThisSpringSemester()
    {
        using (TemporaryTime.Is(new DateTime(2019, 3, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-1), membership: true, training: false, classes: false)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeFalse();
            user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
        }
    }

    [Test]
    public void TestPaidTrainingThisSpringSemester()
    {
        using (TemporaryTime.Is(new DateTime(2019, 3, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-1), membership: true, training: true, classes: false)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeTrue();
            user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
        }
    }

    [Test]
    public void TestPaidMembershipThisSpringAndTrainingThisFallSemester()
    {
        using (TemporaryTime.Is(new DateTime(2019, 10, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-6), membership: true, training: false, classes: false),
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-1), membership: false, training: true, classes: false)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeTrue();
            user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
        }
    }

    [Test]
    public void TestPaidMembershipThisSpringAndClassesThisFallSemester()
    {
        using (TemporaryTime.Is(new DateTime(2019, 10, 2)))
        {
            var user = new User
            {
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-6), membership: true, training: false, classes: false),
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-1), membership: false, training: true, classes: true)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeTrue();
            user.HasPayedClassesFeeThisSemester().ShouldBeTrue();
        }
    }

    [Test]
    public void TestPaidExemptFromFees()
    {
        using (TemporaryTime.Is(new DateTime(2019, 10, 2)))
        {
            var user = new User
            {
                ExemptFromClassesFee = true,
                ExemptFromTrainingFee = true
            };

            user.HasPayedMembershipThisYear().ShouldBeFalse();
            user.HasPayedTrainingFeeThisSemester().ShouldBeFalse();
            user.HasPayedClassesFeeThisSemester().ShouldBeFalse();
        }
    }

    [Test]
    public void TestPaidExemptFromFees_PaidMembership()
    {
        using (TemporaryTime.Is(new DateTime(2019, 10, 2)))
        {
            var user = new User
            {
                ExemptFromClassesFee = true,
                ExemptFromTrainingFee = true,
                Payments =
                    {
                        Payment(paidAt: TimeProvider.UtcNow.AddMonths(-6), membership: true, training: false, classes: false)
                    }
            };

            user.HasPayedMembershipThisYear().ShouldBeTrue();
            user.HasPayedTrainingFeeThisSemester().ShouldBeTrue();
            user.HasPayedClassesFeeThisSemester().ShouldBeTrue();
        }
    }

    private static Payment Payment(DateTime? paidAt = null, bool refunded = false, bool membership = false, bool training = false, bool classes = false) => new Payment
    {
        IncludesMembership = membership,
        IncludesTraining = training,
        IncludesClasses = classes,
        PayedAtUtc = paidAt ?? TimeProvider.UtcNow,
        Refunded = refunded
    };
}
