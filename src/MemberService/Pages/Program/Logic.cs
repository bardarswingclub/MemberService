using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clave.Expressionify;
using MemberService.Data;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;

namespace MemberService.Pages.Program
{
    public static class Logic
    {
        public static async Task<IReadOnlyList<ProgramModel>> GetPrograms(this MemberContext database, bool archived = false)
            => await database.Programs
                .Expressionify()
                .Where(e => archived || e.Archived == false)
                .OrderByDescending(e => e.CreatedAt)
                .Select(p => ProgramModel.Create(p))
                .ToListAsync();

        public static async Task<ProgramModel> GetProgram(this MemberContext database, int id)
            => ProgramModel.Create(await database.FindAsync<Data.Program>(id));

        public static async Task<ProgramInputModel> GetProgramInputModel(this MemberContext database, int id)
        {
            var model = await database.FindAsync<Data.Program>(id);

            if (model == null) return null;

            var (signupOpensAtDate, signupOpensAtTime) = model.SignupOpensAt.GetLocalDateAndTime();
            var (signupClosesAtDate, signupClosesAtTime) = model.SignupClosesAt.GetLocalDateAndTime();

            return new ProgramInputModel
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                EnableSignupOpensAt = model.SignupOpensAt.HasValue,
                SignupOpensAtDate = signupOpensAtDate,
                SignupOpensAtTime = signupOpensAtTime,
                EnableSignupClosesAt = model.SignupClosesAt.HasValue,
                SignupClosesAtDate = signupClosesAtDate,
                SignupClosesAtTime = signupClosesAtTime,
            };
        }

        public static async Task<int> AddProgram(this MemberContext database, ProgramInputModel input, MemberUser user)
        {
            var entry = database.Add(new Data.Program
            {
                CreatedAt = DateTime.UtcNow,
                CreatedByUser = user,
                Title = input.Title,
                Description = input.Description,
                Type = input.Type,
                SignupOpensAt = GetUtc(input.EnableSignupOpensAt, input.SignupOpensAtDate, input.SignupOpensAtTime),
                SignupClosesAt = GetUtc(input.EnableSignupClosesAt, input.SignupClosesAtDate, input.SignupClosesAtTime),
                Archived = false
            });

            await database.SaveChangesAsync();

            return entry.Entity.Id;
        }

        public static void UpdateProgram(this Data.Program entity, ProgramInputModel model)
        {
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.SignupOpensAt = GetUtc(model.EnableSignupOpensAt, model.SignupOpensAtDate, model.SignupOpensAtTime);
            entity.SignupClosesAt = GetUtc(model.EnableSignupClosesAt, model.SignupClosesAtDate, model.SignupClosesAtTime);
            entity.Type = model.Type;
        }


        public static void SetProgramStatus(this Data.Program model, string status)
        {
            if (status == "open")
            {
                model.SignupOpensAt = DateTime.UtcNow;
                model.SignupClosesAt = null;
            }
            else if (status == "close")
            {
                model.SignupClosesAt = DateTime.UtcNow;
            }
            else if (status == "archive")
            {
                model.SignupClosesAt = model.SignupClosesAt ?? DateTime.UtcNow;
                model.Archived = true;
            }
        }

        public static Task EditProgram(this MemberContext context, int id, Action<Data.Program> action)
            => context.EditProgram(id, e =>
            {
                action(e);
                return Task.CompletedTask;
            });

        public static async Task EditProgram(this MemberContext context, int id, Func<Data.Program, Task> action)
        {
            var entry = await context.Programs
                .SingleOrDefaultAsync(e => e.Id == id);

            if (entry == null) return;

            await action(entry);

            await context.SaveChangesAsync();
        }

        public static DateTime? GetUtc(bool enable, string date, string time)
        {
            if (!enable) return null;

            return LocalDateTimePattern.GeneralIso.Parse($"{date}T{time}:00")
                .GetValueOrThrow()
                .InZoneLeniently(Constants.TimeZoneOslo)
                .ToDateTimeUtc();
        }
    }
}
