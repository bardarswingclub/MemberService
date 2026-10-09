namespace MemberService.Data.CoursePlanning;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CoursePlanConfiguration : IEntityTypeConfiguration<CoursePlan>
{
    public void Configure(EntityTypeBuilder<CoursePlan> builder)
    {
        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.RoomsJson)
            .IsRequired();

        builder.Property(p => p.HolidaysJson)
            .IsRequired()
            .HasDefaultValue("[]");

        builder.HasMany(p => p.Courses)
            .WithOne(c => c.CoursePlan)
            .HasForeignKey(c => c.CoursePlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PlannedCourseConfiguration : IEntityTypeConfiguration<PlannedCourse>
{
    public void Configure(EntityTypeBuilder<PlannedCourse> builder)
    {
        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Color).HasMaxLength(20);
        builder.Property(c => c.RoomId).HasMaxLength(50);
        builder.Property(c => c.SlotId).HasMaxLength(50);
        builder.Property(c => c.StartTime).HasMaxLength(5);
        builder.Property(c => c.EndTime).HasMaxLength(5);

        builder.HasOne(c => c.Event)
            .WithMany()
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
