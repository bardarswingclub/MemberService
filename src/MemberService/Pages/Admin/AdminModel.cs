namespace MemberService.Pages.Admin;

using MemberService.Data;



public class AdminModel
{
    public IReadOnlyCollection<(MemberRole, IReadOnlyCollection<User>)> Roles { get; set; }
}
