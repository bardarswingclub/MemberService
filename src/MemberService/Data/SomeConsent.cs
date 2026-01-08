namespace MemberService.Data;

using Microsoft.AspNetCore.Identity;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

// Social Media Consent State
public enum SomeConsentState
{
    [Display(Name = "Samtykker ikke")]
    None = 0,

    [Display(Name = "Spør hver gang")]
    Ask = 1,

    [Display(Name = "Jeg samtykker, ikke spør meg")]
    Always = 2
}


public static class SomeConsentStateExtensions
{
    public static string GetDisplayName(this SomeConsentState state)
    {
        var type = typeof(SomeConsentState);
        var memInfo = type.GetMember(state.ToString());
        var attr = memInfo[0]
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;
        return attr?.Name ?? state.ToString();
    }
}


// Social Media Consent Record, keep track of when changed
public class SomeConsentRecord
{
    [Key]
    public long Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    public User User { get; set; } = null!;

    [Required]
    public DateTime ChangedAtUtc { get; set; }

    [Required]
    public SomeConsentState State { get; private set; }

    private SomeConsentRecord() { } // for entity framework

    public SomeConsentRecord(SomeConsentState state, string userId)
    {
        State = state;
        UserId = userId;
        ChangedAtUtc = DateTime.UtcNow;
    }
}

public class SomeConsentRecordConfiguration
    : IEntityTypeConfiguration<SomeConsentRecord>
{
    public void Configure(EntityTypeBuilder<SomeConsentRecord> entity)
    {
        entity.HasKey(e => e.Id);

        entity.HasIndex(e => new { e.UserId, e.ChangedAtUtc })
              .IsUnique();

        entity.HasOne(e => e.User)
              .WithMany(u => u.ConsentRecords)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}