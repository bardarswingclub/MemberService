namespace MemberService.Data;

using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EventOrganizer : IEntityTypeConfiguration<EventOrganizer>
{
    public Guid EventId { get; set; }

    public Event Event { get; set; }

    public string UserId { get; set; }

    public User User { get; set; }

    public bool CanEdit { get; set; }

    public bool CanEditSignup { get; set; }

    public bool CanSetSignupStatus { get; set; }

    public bool CanEditOrganizers { get; set; }

    public bool CanSetPresence { get; set; }

    public bool CanAddPresenceLesson { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string UpdatedBy { get; set; }

    [ForeignKey(nameof(UpdatedBy))]
    public User UpdatedByUser { get; set; }

    public void Configure(EntityTypeBuilder<EventOrganizer> organizer)
    {
        organizer
            .ToTable(nameof(MemberContext.EventOrganizers), b => b.IsTemporal());

        organizer
            .HasKey(nameof(EventId), nameof(UserId));

        organizer
            .HasOne(s => s.User)
            .WithMany(u => u.Organizes)
            .HasForeignKey(s => s.UserId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired(true);

        organizer
            .HasOne(s => s.Event)
            .WithMany(e => e.Organizers)
            .HasForeignKey(s => s.EventId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired(true);
    }
}
