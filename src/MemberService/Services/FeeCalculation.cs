using System;
using MemberService.Data;

namespace MemberService.Services
{
    public enum FeeStatus
    {
        Unpaid,
        Paid,
        Unpayable,
    }

    public static class FeeCalculation
    {
        public const decimal MembershipFee = 300;
        public const decimal TrainingFee = 425;
        public const decimal ClassesFee = 900;

        public static (FeeStatus FeeStatus, Fee Fee) GetFee(this User user, string key)
        {
            switch (key)
            {
                case Fee.Membership:
                    return user.GetMembershipFee();
                case Fee.Training:
                    return user.GetTrainingFee();
                case Fee.Classes:
                    return user.GetClassesFee();
                default:
                    throw new Exception();
            }
        }

        public static (FeeStatus FeeStatus, Fee Fee) GetMembershipFee(this User user)
        {
            if (user.HasPayedMembershipThisYear())
                return (FeeStatus.Paid, null);

            return (
                FeeStatus.Unpaid,
                new Fee("Medlemskap", MembershipFee, includesMembership: true)
            );
        }

        public static (FeeStatus FeeStatus, Fee Fee) GetTrainingFee(this User user)
        {
            if (user.HasPayedTrainingFeeThisSemester())
                return (FeeStatus.Paid, null);

            if (user.ExemptFromTrainingFee)
                return (FeeStatus.Unpayable, null);

            if (user.HasPayedMembershipThisYear())
                return (
                    FeeStatus.Unpaid,
                    new Fee("Treningsavgift", TrainingFee, includesTraining: true)
                );

            return (
                FeeStatus.Unpaid,
                new Fee("Medlemskap og treningsavgift", MembershipFee + TrainingFee, includesMembership: true, includesTraining: true)
            );
        }

        public static (FeeStatus FeeStatus, Fee Fee) GetClassesFee(this User user)
        {
            if (user.HasPayedClassesFeeThisSemester())
                return (FeeStatus.Paid, null);

            if (user.ExemptFromClassesFee)
                return (FeeStatus.Unpayable, null);

            if (user.HasPayedTrainingFeeThisSemester())
                return (
                    FeeStatus.Unpaid,
                    new Fee("Kursavgift", ClassesFee - TrainingFee, includesClasses: true)
                );

            if (user.HasPayedMembershipThisYear())
            {
                if (user.ExemptFromTrainingFee)
                    return (
                        FeeStatus.Unpaid,
                        new Fee("Kursavgift", ClassesFee - TrainingFee, includesClasses: true)
                    );

                return (
                    FeeStatus.Unpaid,
                    new Fee("Kursavgift", ClassesFee, includesTraining: true, includesClasses: true)
                );
            }

            if (user.ExemptFromTrainingFee)
                return (
                    FeeStatus.Unpaid,
                    new Fee("Medlemskap og kursavgift", ClassesFee - TrainingFee + MembershipFee, includesMembership: true, includesClasses: true)
                );

            return (
                FeeStatus.Unpaid,
                new Fee("Medlemskap og kursavgift", MembershipFee + ClassesFee, includesMembership: true, includesTraining: true, includesClasses: true)
            );
        }
    }
}