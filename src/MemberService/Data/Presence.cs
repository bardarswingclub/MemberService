namespace MemberService.Data;

using MemberService.Data.ValueTypes;



public class Presence
{
    public Guid Id { get; set; }

    public Guid SignupId { get; set; }

    public EventSignup Signup { get; set; }

    public int Lesson { get; set; }

    public bool Present { get; set; }

    public AbsenceReason? AbsenceReason { get; set; }

    public string AbsenceNote { get; set; }

    public DateTime RegisteredAt { get; set; }

    public string RegisteredById { get; set; }

    public User RegisteredBy { get; set; }
}
