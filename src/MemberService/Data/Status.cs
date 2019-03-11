using System.ComponentModel.DataAnnotations;

namespace MemberService.Data
{
    public enum Status
    {
        Unknown = 0,
        Pending = 1,
        Recommended = 2,
        WaitingList = 3,
        Approved = 4,
        AcceptedAndPayed = 5,
        RejectedOrNotPayed = 6,
        Denied = 7
    }
}