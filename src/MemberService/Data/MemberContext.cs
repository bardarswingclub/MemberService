using System;
using System.Security.Claims;
using System.Threading.Tasks;
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
                    .HasConversion(
                        v => v.ToString(),
                        v => (DanceRole)Enum.Parse(typeof(DanceRole), v));

                signup
                    .Property(s => s.Status)
                    .HasConversion(
                        v => v.ToString(),
                        v => (Status)Enum.Parse(typeof(Status), v));
            });
        }
    }
}
