namespace MemberService.Data;



using Microsoft.AspNetCore.Identity;

public class User : IdentityUser
{
    [PersonalData]
    public string FullName { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<EventSignup> EventSignups { get; set; } = new List<EventSignup>();

    public ICollection<EventOrganizer> Organizes { get; set; } = new List<EventOrganizer>();

    public bool ExemptFromTrainingFee { get; set; }

    public bool ExemptFromClassesFee { get; set; }
}
