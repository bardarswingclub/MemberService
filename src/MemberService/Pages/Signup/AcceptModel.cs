namespace MemberService.Pages.Signup;



public class AcceptModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public bool MustPayClassesFee { get; set; }

    public bool MustPayTrainingFee { get; set; }

    public bool MustPayMembershipFee { get; set; }

    public decimal MustPayAmount { get; set; }

    public string SessionId { get; set; }
}
