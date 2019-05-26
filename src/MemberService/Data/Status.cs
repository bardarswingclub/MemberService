using System.ComponentModel;

namespace MemberService.Data
{
    public enum Status
    {
        Unknown = 0,

        [DisplayName("P�meldt")]
        Pending = 1,

        [DisplayName("Anbefalt plass")]
        Recommended = 2,

        [DisplayName("P� venteliste")]
        WaitingList = 3,

        [DisplayName("Gitt plass")]
        Approved = 4,

        [DisplayName("Godtatt og betalt")]
        AcceptedAndPayed = 5,

        [DisplayName("Takket nei til plass")]
        RejectedOrNotPayed = 6,

        [DisplayName("Fratatt plass")]
        Denied = 7
    }
}