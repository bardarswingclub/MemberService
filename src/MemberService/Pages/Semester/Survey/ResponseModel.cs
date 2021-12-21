namespace MemberService.Pages.Semester.Survey;



using Clave.Expressionify;

using MemberService.Data;

public partial class ResponseModel
{
    public string Name { get; set; }

    public string UserId { get; set; }

    public bool HasPayedMembershipThisYear { get; set; }

    public bool HasPayedTrainingFeeThisSemester { get; set; }

    public bool HasPayedClassesFeeThisSemester { get; set; }

    public Guid OptionId { get; set; }

    [Expressionify]
    public static ResponseModel Create(Response r, QuestionAnswer a) =>
        new()
        {
            UserId = r.UserId,
            Name = r.User.FullName,
            HasPayedMembershipThisYear = r.User.HasPayedMembershipThisYear(),
            HasPayedTrainingFeeThisSemester = r.User.HasPayedTrainingFeeThisSemester(),
            HasPayedClassesFeeThisSemester = r.User.HasPayedClassesFeeThisSemester(),
            OptionId = a.OptionId
        };
}
