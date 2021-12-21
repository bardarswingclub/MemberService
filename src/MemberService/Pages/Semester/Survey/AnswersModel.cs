namespace MemberService.Pages.Semester.Survey;

using Clave.Expressionify;

using MemberService.Data.ValueTypes;

public partial class AnswerModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public Status Status { get; set; }

    [Expressionify]
    public static AnswerModel Create(Data.EventSignup s) =>
        new()
        {
            Id = s.UserId,
            Name = s.User.FullName,
            Status = s.Status
        };
}
