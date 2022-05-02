namespace MemberService.Data;

using System.Linq.Expressions;
using System.Security.Claims;

using Clave.Expressionify;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static partial class Extensions
{
    [Expressionify]
    public static bool HasPayedMembershipThisYear(this User user)
        => user.Payments.Any(p => p.PayedAtUtc > TimeProvider.ThisYearUtc && p.IncludesMembership && !p.Refunded);

    [Expressionify]
    public static bool HasPayedMembershipLastOrThisYear(this User user)
        => user.Payments.Any(p => p.PayedAtUtc > TimeProvider.LastYearUtc && p.IncludesMembership && !p.Refunded);

    [Expressionify]
    public static bool HasPayedMembershipLastYear(this User user)
        => user.Payments.Any(p => p.PayedAtUtc > TimeProvider.LastYearUtc && p.PayedAtUtc < TimeProvider.ThisYearUtc && p.IncludesMembership && !p.Refunded);

    [Expressionify]
    public static bool HasPayedTrainingFeeThisSemester(this User user)
        => user.Payments.Any(p => p.PayedAtUtc > TimeProvider.ThisSemesterUtc && p.IncludesTraining && !p.Refunded)
        || user.ExemptFromTrainingFee && user.HasPayedMembershipThisYear();

    [Expressionify]
    public static bool HasPayedClassesFeeThisSemester(this User user)
        => user.Payments.Any(p => p.PayedAtUtc > TimeProvider.ThisSemesterUtc && p.IncludesClasses && !p.Refunded)
        || user.ExemptFromClassesFee && user.HasPayedTrainingFeeThisSemester();

    [Expressionify]
    public static bool HasOpened(this Event e)
        => e.SignupOptions.SignupOpensAt == null || e.SignupOptions.SignupOpensAt < TimeProvider.UtcNow;

    [Expressionify]
    public static bool HasClosed(this Event e)
        => e.SignupOptions.SignupClosesAt != null && e.SignupOptions.SignupClosesAt < TimeProvider.UtcNow;

    [Expressionify]
    public static bool IsOpen(this Event e)
        => e.HasOpened() && !e.HasClosed();

    public static bool IsLive(this AnnualMeeting m)
        => m.MeetingStartsAt < TimeProvider.UtcNow && m.MeetingEndsAt > TimeProvider.UtcNow;

    [Expressionify]
    public static bool WillOpen(this Event e)
        => !e.HasOpened() && !e.HasClosed();

    [Expressionify]
    public static bool IsSignedUpFor(this User user, Guid id)
        => user.EventSignups.Any(e => e.EventId == id);

    [Expressionify]
    public static bool IsActive(this Semester semester)
        => semester.SignupOpensAt > TimeProvider.ThisSemesterUtc;

    [Expressionify]
    public static bool NameMatches(this User user, string name)
        => user.FullName.Contains(name) | user.Email.Contains(name);

    public static async Task<User> SingleUser(this IQueryable<User> users, ClaimsPrincipal user)
        => await users.SingleUser(user.GetId());

    public static async Task<User> Get(this MemberContext database, ClaimsPrincipal user)
        => await database.Users.SingleUser(user.GetId());

    public static async Task<User> SingleUser(this IQueryable<User> users, string id)
        => await users.SingleOrDefaultAsync(user => user.Id == id);

    public static void HasEnumStringConversion<TEnum>(this PropertyBuilder<TEnum> property)
        where TEnum : struct
        => property.HasConversion(
                v => v.ToString(),
                v => Enum.Parse<TEnum>(v));

    public static async Task<Semester> Current(this DbSet<Semester> semesters)
    {
        return await semesters.Current(t => t);
    }

    public static async Task<T> Current<T>(this DbSet<Semester> semesters, Expression<Func<Semester, T>> select)
    {
        return await semesters
            .Where(s => s.IsActive())
            .OrderByDescending(s => s.SignupOpensAt)
            .Select(select)
            .FirstOrDefaultAsync();
    }
}
