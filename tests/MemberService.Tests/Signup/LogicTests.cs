namespace MemberService.Tests.Signup;

using Clave.ExtensionMethods.Magic;

using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Signup;

using NUnit.Framework;

[TestFixture]
public class LogicTests
{
    [TestCase(0, 0, DanceRole.Follow, ExpectedResult = false)]
    [TestCase(10, 0, DanceRole.Follow, ExpectedResult = true)]
    [TestCase(10, 10, DanceRole.Follow, ExpectedResult = true)]
    [TestCase(0, 0, DanceRole.Lead, ExpectedResult = false)]
    [TestCase(10, 0, DanceRole.Lead, ExpectedResult = true)]
    [TestCase(10, 10, DanceRole.Lead, ExpectedResult = false)]
    public bool TestShouldAutoAccept(int autoAcceptCount, int signupCount, DanceRole role)
    {
        var model = new Event
        {
            SignupOptions = new()
            {
                AutoAcceptedSignups = autoAcceptCount
            },
            Signups =
            {
                GenerateSignups(signupCount)
            }
        };

        return model.ShouldAutoAccept(role);
    }

    [TestCase(Status.Unknown, ExpectedResult = false)]
    [TestCase(Status.Pending, ExpectedResult = true)]
    [TestCase(Status.Recommended, ExpectedResult = true)]
    [TestCase(Status.Approved, ExpectedResult = false)]
    [TestCase(Status.WaitingList, ExpectedResult = true)]
    [TestCase(Status.AcceptedAndPayed, ExpectedResult = false)]
    [TestCase(Status.Denied, ExpectedResult = false)]
    [TestCase(Status.RejectedOrNotPayed, ExpectedResult = false)]
    public bool TestCanEdit(Status status)
    {
        var model = new EventSignup
        {
            Status = status
        };

        return model.CanEdit();
    }

    [TestCase( 0,  0, false, false, ExpectedResult = SignupRequirement.None)]
    [TestCase( 0,  0,  true, false, ExpectedResult = SignupRequirement.None)]
    [TestCase(10, 10, false, false, ExpectedResult = SignupRequirement.MustPayNonMembersPrice)]
    [TestCase(10, 10,  true, false, ExpectedResult = SignupRequirement.MustPayMembersPrice)]
    public SignupRequirement TestGetRequirement(
        int membersPrice,
        int nonMembersPrice,
        bool membership,
        bool training)
    {
        var user = new User
        {
            Payments =
            {
                Payment(membership: membership, training: training)
            }
        };

        return user.GetRequirement(new()
        {
            PriceForMembers = membersPrice,
            PriceForNonMembers = nonMembersPrice,
            IncludedInClassesFee = false,
            IncludedInTrainingFee = false,
            RequiresClassesFee = false,
            RequiresTrainingFee = false,
            RequiresMembershipFee = false,
        });
    }

    [TestCase(0, false, ExpectedResult = false)]
    [TestCase(0, true, ExpectedResult = false)]
    [TestCase(100, false, ExpectedResult = true)]
    [TestCase(100, true, ExpectedResult = false)]
    public bool TestMustPayNonMembersPrice(int price, bool hasPaidMembership)
    {
        var user = new User
        {
            Payments =
            {
                Payment(membership: hasPaidMembership)
            }
        };

        return user.MustPayNonMembersPrice(new()
        {
            PriceForNonMembers = price
        });
    }

    [TestCase(0, false, ExpectedResult = false)]
    [TestCase(0, true, ExpectedResult = false)]
    [TestCase(100, false, ExpectedResult = false)]
    [TestCase(100, true, ExpectedResult = true)]
    public bool TestMustPayMembersPrice(int price, bool hasPaidMembership)
    {
        var user = new User
        {
            Payments =
            {
                Payment(membership: hasPaidMembership)
            }
        };

        return user.MustPayMembersPrice(new()
        {
            PriceForMembers = price
        });
    }

    [TestCase(false, false, ExpectedResult = false)]
    [TestCase(false, true, ExpectedResult = false)]
    [TestCase(true, false, ExpectedResult = true)]
    [TestCase(true, true, ExpectedResult = false)]
    public bool TestMustPayMembersFee(bool requiresMembership, bool hasPaidMembership)
    {
        var user = new User
        {
            Payments =
                {
                    Payment(membership: hasPaidMembership)
                }
        };

        return user.MustPayMembershipFee(new()
        {
            RequiresMembershipFee = requiresMembership
        });
    }

    [TestCase(false, false, false, ExpectedResult = false)]
    [TestCase(false, false, true, ExpectedResult = false)]
    [TestCase(false, true, false, ExpectedResult = false)]
    [TestCase(false, true, true, ExpectedResult = false)]
    [TestCase(true, false, false, ExpectedResult = true)]
    [TestCase(true, false, true, ExpectedResult = false)]
    [TestCase(true, true, false, ExpectedResult = false)]
    [TestCase(true, true, true, ExpectedResult = false)]
    public bool TestMustPayTrainingFee(bool requiresTrainingFee, bool hasPaidTrainingFee, bool exemptFromTrainingFee)
    {
        var user = new User
        {
            ExemptFromTrainingFee = exemptFromTrainingFee,
            Payments =
                {
                    Payment(training: hasPaidTrainingFee)
                }
        };

        return user.MustPayTrainingFee(new()
        {
            RequiresTrainingFee = requiresTrainingFee
        });
    }

    [TestCase(false, false, false, ExpectedResult = false)]
    [TestCase(false, false, true, ExpectedResult = false)]
    [TestCase(false, true, false, ExpectedResult = false)]
    [TestCase(false, true, true, ExpectedResult = false)]
    [TestCase(true, false, false, ExpectedResult = true)]
    [TestCase(true, false, true, ExpectedResult = false)]
    [TestCase(true, true, false, ExpectedResult = false)]
    [TestCase(true, true, true, ExpectedResult = false)]
    public bool TestMustPayClassesFee(bool requiresClassesFee, bool hasPaidClassesFee, bool exemptFromClassesFee)
    {
        var user = new User
        {
            ExemptFromClassesFee = exemptFromClassesFee,
            Payments =
                {
                    Payment(classes: hasPaidClassesFee)
                }
        };

        return user.MustPayClassesFee(new()
        {
            RequiresClassesFee = requiresClassesFee
        });
    }

    [TestCase(false, false, ExpectedResult = true)]
    [TestCase(false, true, ExpectedResult = true)]
    [TestCase(true, false, ExpectedResult = true)]
    [TestCase(true, true, ExpectedResult = false)]
    public bool TestNonMembershipIncludedInClassesFee(bool includedInClassesFee, bool hasPaidClassesFee)
    {
        var user = new User
        {
            Payments =
                {
                    Payment(classes: hasPaidClassesFee)
                }
        };

        return user.MustPayNonMembersPrice(new()
        {
            IncludedInClassesFee = includedInClassesFee,
            PriceForNonMembers = 100
        });
    }

    [TestCase(false, false, ExpectedResult = true)]
    [TestCase(false, true, ExpectedResult = true)]
    [TestCase(true, false, ExpectedResult = true)]
    [TestCase(true, true, ExpectedResult = false)]
    public bool TestMembershipIncludedInClassesFee(bool includedInClassesFee, bool hasPaidClassesFee)
    {
        var user = new User
        {
            Payments =
                {
                    Payment(classes: hasPaidClassesFee, membership: true)
                }
        };

        return user.MustPayMembersPrice(new()
        {
            IncludedInClassesFee = includedInClassesFee,
            PriceForMembers = 100
        });
    }

    [TestCase(false, false, ExpectedResult = true)]
    [TestCase(false, true, ExpectedResult = true)]
    [TestCase(true, false, ExpectedResult = true)]
    [TestCase(true, true, ExpectedResult = false)]
    public bool TestNonMembershipIncludedInTrainingFee(bool includedInTrainingFee, bool hasPaidTrainingFee)
    {
        var user = new User
        {
            Payments =
                {
                    Payment(training: hasPaidTrainingFee)
                }
        };

        return user.MustPayNonMembersPrice(new()
        {
            IncludedInTrainingFee = includedInTrainingFee,
            PriceForNonMembers = 100
        });
    }

    [TestCase(false, false, ExpectedResult = true)]
    [TestCase(false, true, ExpectedResult = true)]
    [TestCase(true, false, ExpectedResult = true)]
    [TestCase(true, true, ExpectedResult = false)]
    public bool TestMembershipIncludedInTrainingFee(bool includedInTrainingFee, bool hasPaidTrainingFee)
    {
        var user = new User
        {
            Payments =
                {
                    Payment(training: hasPaidTrainingFee, membership: true)
                }
        };

        return user.MustPayMembersPrice(new()
        {
            IncludedInTrainingFee = includedInTrainingFee,
            PriceForMembers = 100
        });
    }

    private static Payment Payment(DateTime? paidAt = null, bool refunded = false, bool membership = false, bool training = false, bool classes = false) => new Payment
    {
        IncludesMembership = membership,
        IncludesTraining = training,
        IncludesClasses = classes,
        PayedAtUtc = paidAt ?? TimeProvider.UtcNow,
        Refunded = refunded
    };

    private static IEnumerable<EventSignup> GenerateSignups(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new EventSignup
            {
                Role = DanceRole.Lead
            };
        }
    }
}
