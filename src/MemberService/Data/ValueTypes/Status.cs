using System.ComponentModel;

namespace MemberService.Data.ValueTypes
{
    public enum Status
    {
        Unknown = 0,

        [DisplayName("Påmeldt")]
        [Description("Påmeldt")]
        Pending = 1,

        [DisplayName("Anbefalt plass")]
        [Description("Påmeldt")]
        Recommended = 2,

        [DisplayName("På venteliste")]
        [Description("På venteliste")]
        WaitingList = 3,

        [DisplayName("Gitt plass")]
        [Description("Fått plass")]
        Approved = 4,

        [DisplayName("Godtatt og betalt")]
        [Description("Godtatt")]
        AcceptedAndPayed = 5,

        [DisplayName("Takket nei til plass")]
        [Description("Takket nei")]
        RejectedOrNotPayed = 6,

        [DisplayName("Ikke fått plass")]
        [Description("Ikke fått plass")]
        Denied = 7
    }
}