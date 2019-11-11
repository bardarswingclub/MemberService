using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Data
{
    public class MemberContext : IdentityDbContext<MemberUser, MemberRole, string, IdentityUserClaim<string>,
    MemberUserRole, IdentityUserLogin<string>,
    IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public MemberContext(DbContextOptions<MemberContext> options)
            : base(options)
        {
        }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<EventSignup> EventSignups { get; set; }

        public DbSet<Presence> Presence { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<QuestionAnswer> QuestionAnswers { get; set; }

        public DbSet<QuestionOption> QuestionOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MemberUserRole>(userRole =>
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

            builder.Entity<Payment>(payment =>
            {
                payment.HasIndex(p => p.PayedAtUtc);
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
                    .HasOne(s => s.Partner)
                    .WithMany()
                    .HasForeignKey(s => s.PartnerEmail)
                    .HasPrincipalKey(s => s.NormalizedEmail)
                    .IsRequired(false);
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
                    .HasOne<QuestionOption>()
                    .WithMany()
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
