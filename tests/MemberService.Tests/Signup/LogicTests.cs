using System;
using System.Collections.Generic;
using Clave.ExtensionMethods.Magic;
using MemberService.Data;
using MemberService.Pages.Signup;
using NUnit.Framework;

namespace MemberService.Tests.Signup
{
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
            var model = new Data.Event
            {
                SignupOptions = new EventSignupOptions
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
        [TestCase(Status.WaitingList, ExpectedResult = false)]
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

        [TestCase(0, false, ExpectedResult = false)]
        [TestCase(0, true, ExpectedResult = false)]
        [TestCase(100, false, ExpectedResult = true)]
        [TestCase(100, true, ExpectedResult = false)]
        public bool TestMustPayNonMembersPrice(int price, bool hasPaidMembership)
        {
            var options = new EventSignupOptions
            {
                PriceForNonMembers = price
            };

            var user = new MemberUser
            {
                Payments =
                {
                    Payment(membership: hasPaidMembership)
                }
            };

            return user.MustPayNonMembersPrice(options);
        }

        [TestCase(0, false, ExpectedResult = false)]
        [TestCase(0, true, ExpectedResult = false)]
        [TestCase(100, false, ExpectedResult = false)]
        [TestCase(100, true, ExpectedResult = true)]
        public bool TestMustPayMembersPrice(int price, bool hasPaidMembership)
        {
            var options = new EventSignupOptions
            {
                PriceForMembers = price
            };

            var user = new MemberUser
            {
                Payments =
                {
                    Payment(membership: hasPaidMembership)
                }
            };

            return user.MustPayMembersPrice(options);
        }

        [TestCase(false, false, ExpectedResult = false)]
        [TestCase(false, true, ExpectedResult = false)]
        [TestCase(true, false, ExpectedResult = true)]
        [TestCase(true, true, ExpectedResult = false)]
        public bool TestMustPayMembersFee(bool requiresMembership, bool hasPaidMembership)
        {
            var options = new EventSignupOptions
            {
                RequiresMembershipFee = requiresMembership
            };

            var user = new MemberUser
            {
                Payments =
                {
                    Payment(membership: hasPaidMembership)
                }
            };

            return user.MustPayMembershipFee(options);
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
            var options = new EventSignupOptions
            {
                RequiresTrainingFee = requiresTrainingFee
            };

            var user = new MemberUser
            {
                ExemptFromTrainingFee = exemptFromTrainingFee,
                Payments =
                {
                    Payment(training: hasPaidTrainingFee)
                }
            };

            return user.MustPayTrainingFee(options);
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
            var options = new EventSignupOptions
            {
                RequiresClassesFee = requiresClassesFee
            };

            var user = new MemberUser
            {
                ExemptFromClassesFee = exemptFromClassesFee,
                Payments =
                {
                    Payment(classes: hasPaidClassesFee)
                }
            };

            return user.MustPayClassesFee(options);
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
}
