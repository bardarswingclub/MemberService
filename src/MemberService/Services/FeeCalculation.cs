using MemberService.Data;

namespace MemberService.Services
{
    public static class FeeCalculation
    {
        public static Fee GetMembershipFee(this MemberUser user)
        {
            if (user.HasPayedMembershipThisYear())
                return null;

            return new Fee("Medlemskap", 300);
        }
    }
}