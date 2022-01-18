namespace MemberService.Data;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SemesterRole : IEntityTypeConfiguration<SemesterRole>
{
    public enum RoleType
    {
        [Display(Name="Instruktør")]
        Instructor,
        [Display(Name="Koordnator")]
        Coordinator
    }

    public Guid SemesterId { get; set; }

    public Semester Semester { get; set; }

    public string UserId { get; set; }

    public User User { get; set; }

    public RoleType Role { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; }

    [ForeignKey(nameof(UpdatedBy))]
    public User UpdatedByUser { get; set; }

    public void Configure(EntityTypeBuilder<SemesterRole> semesterRoles)
    {
        semesterRoles
            .ToTable(nameof(MemberContext.SemesterRoles), b => b.IsTemporal());

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