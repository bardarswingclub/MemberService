namespace MemberService.Pages.Home;




using System.Linq.Expressions;


using Clave.Expressionify;

using MemberService.Data;

using Microsoft.EntityFrameworkCore;

public static class Logic
{
    public static async Task<IReadOnlyList<CourseModel>> GetCourses(this MemberContext db, string userId, Expression<Func<Data.Event, bool>> predicate)
        => await db.Events
            .Include(e => e.SignupOptions)
            .Include(e => e.Semester)
            .AsNoTracking()
            .Expressionify()
            .Where(e => e.Semester != null)
            .Where(e => e.Semester.IsActive())
            .Where(e => e.Archived == false)
            .Where(predicate)
            .OrderBy(e => e.SignupOptions.SignupOpensAt)
            .Select(e => CourseModel.Create(e, userId))
            .ToListAsync();
}
