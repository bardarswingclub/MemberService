﻿namespace MemberService.Data;

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class MemberContext : IdentityDbContext<User, MemberRole, string, IdentityUserClaim<string>, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>, IDataProtectionKeyContext
{
    public MemberContext(DbContextOptions<MemberContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; }

    public DbSet<Event> Events { get; set; }

    public DbSet<EventSignup> EventSignups { get; set; }

    public DbSet<Presence> Presence { get; set; }

    public DbSet<Survey> Surveys { get; set; }

    public DbSet<Question> Questions { get; set; }

    public DbSet<QuestionAnswer> QuestionAnswers { get; set; }

    public DbSet<QuestionOption> QuestionOptions { get; set; }

    public DbSet<Semester> Semesters { get; set; }

    public DbSet<AnnualMeeting> AnnualMeetings { get; set; }

    public DbSet<EventOrganizer> EventOrganizers { get; set; }

    public DbSet<SemesterRole> SemesterRoles { get; set; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public DbSet<VippsReservation> VippsReservations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(MemberContext).Assembly);

        builder.Entity<UserRole>(userRole =>
        {
            userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

            userRole.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            userRole.HasOne(ur => ur.User)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        builder.Entity<EventSignupOptions>(options =>
        {
            options.HasIndex(o => o.SignupOpensAt);
            options.HasIndex(o => o.SignupClosesAt);
        });

        builder.Entity<EventSignup>(signup =>
        {
            signup
                .Property(s => s.Role)
                .HasEnumStringConversion();

            signup
                .Property(s => s.Status)
                .HasEnumStringConversion();

            signup
                .HasOne(s => s.User)
                .WithMany(u => u.EventSignups)
                .HasForeignKey(s => s.UserId)
                .HasPrincipalKey(u => u.Id)
                .IsRequired(true);
        });

        builder.Entity<Event>(@event =>
        {
            @event
                .Property(e => e.Type)
                .HasEnumStringConversion();
        });

        builder.Entity<Question>(question =>
        {
            question
                .Property(q => q.Type)
                .HasEnumStringConversion();
        });

        builder.Entity<QuestionAnswer>(answer =>
        {
            answer
                .HasOne(q => q.Option)
                .WithMany(o => o.Answers)
                .HasForeignKey(q => q.OptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
