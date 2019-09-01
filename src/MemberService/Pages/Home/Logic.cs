using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clave.Expressionify;
using MemberService.Data;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Home
{
    public static class Logic
    {
        public static async Task<IReadOnlyList<ClassModel>> GetClasses(this MemberContext db, string userId, Expression<Func<Data.Event, bool>> predicate)
            => await db.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Where(e => e.Type == EventType.Class)
                .Where(e => e.Archived == false)
                .Where(predicate)
                .OrderBy(e => e.SignupOptions.SignupOpensAt)
                .Select(e => ClassModel.Create(e, userId))
                .ToListAsync();
    }
}
