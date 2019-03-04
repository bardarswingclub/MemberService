using System;
using MemberService.Data;

namespace MemberService.Services
{
    public static class FeeCalculation
    {
        public const decimal MembershipFee = 300;
        public const decimal TrainingFee = 125;
        public const decimal ClassesFee = 900;

        public static Fee GetFee(this MemberUser user, string key)
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

        public static Fee GetMembershipFee(this MemberUser user)
        {
            if (user.HasPayedMembershipThisYear())
                return null;

            return new Fee("Medlemskap", MembershipFee)
            {
                IncludesMembership = true
            };
        }

        public static Fee GetTrainingFee(this MemberUser user)
        {
            if (user.HasPayedTrainingThisSemester())
                return null;

            if (user.HasPayedMembershipThisYear())
                return new Fee("Treningsavgift", TrainingFee)
                {
                    IncludesTraining = true
                };

            return new Fee("Medlemskap og treningsavgift", MembershipFee + TrainingFee)
            {
                IncludesMembership = true,
                IncludesTraining = true
            };
        }

        public static Fee GetClassesFee(this MemberUser user)
        {
            if (user.HasPayedClassesThisSemester())
                return null;

            if (user.HasPayedTrainingThisSemester())
                return new Fee("Kursavgift", ClassesFee - TrainingFee)
                {
                    IncludesClasses = true
                };

            if (user.HasPayedMembershipThisYear())
                return new Fee("Kursavgift", ClassesFee)
                {
                    IncludesTraining = true,
                    IncludesClasses = true
                };

            return new Fee("Medlemskap og kursavgift", MembershipFee + ClassesFee)
            {
                IncludesMembership = true,
                IncludesTraining = true,
                IncludesClasses = true
            };
        }
    }
}