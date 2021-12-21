namespace MemberService.Pages.Members;



using MemberService.Data;

public class MembersViewModel
{
    public IReadOnlyCollection<(char Letter, IReadOnlyCollection<User> Users)> Users { get; set; }

    public string Query { get; set; }

    public string MemberFilter { get; set; }

    public string TrainingFilter { get; set; }

    public string ClassesFilter { get; set; }

    public string ExemptTrainingFilter { get; set; }

    public string ExemptClassesFilter { get; set; }
}
