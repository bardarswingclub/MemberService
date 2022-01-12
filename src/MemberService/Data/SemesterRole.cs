namespace MemberService.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SemesterRole : IEntityTypeConfiguration<SemesterRole>
{
    public enum RoleType
    {
        Unknown,
        ViewOnly,
        Instructor,
        Coordinator
    }

    public Guid SemesterId { get; set; }

    public Semester Semester { get; set; }

    public string UserId { get; set; }

    public User User { get; set; }

    public RoleType Role { get; set; }

    public void Configure(EntityTypeBuilder<SemesterRole> semesterRoles)
    {
        semesterRoles
            .HasKey(nameof(SemesterId), nameof(UserId));

        semesterRoles
            .Property(s => s.Role)
            .HasEnumStringConversion();

        semesterRoles
            .HasOne(s => s.User)
            .WithMany(u => u.SemesterRoles)
            .HasForeignKey(s => s.UserId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired(true);

        semesterRoles
            .HasOne(s => s.Semester)
            .WithMany(e => e.Roles)
            .HasForeignKey(s => s.SemesterId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired(true);
    }
}