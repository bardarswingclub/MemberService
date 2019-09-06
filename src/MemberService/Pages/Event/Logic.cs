using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using MemberService.Data;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;

namespace MemberService.Pages.Event
{
    public static class Logic
    {
        public static Task<List<Data.Event>> GetEvents(this MemberContext context, bool archived)
            => context.Events
                .Include(e => e.SignupOptions)
                .Include(e => e.Signups)
                .AsNoTracking()
                .Where(e => archived || e.Archived == false)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

        public static Data.Event ToEntity(this EventInputModel model, MemberUser user)
            => new Data.Event
            {
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                CreatedAt = TimeProvider.UtcNow,
                CreatedByUser = user,
                SignupOptions = new EventSignupOptions
                {
                    RequiresMembershipFee = model.RequiresMembershipFee,
                    RequiresTrainingFee = model.RequiresTrainingFee,
                    RequiresClassesFee = model.RequiresClassesFee,
                    PriceForMembers = model.PriceForMembers,
                    PriceForNonMembers = model.PriceForNonMembers,
                    SignupOpensAt = GetUtc(model.EnableSignupOpensAt, model.SignupOpensAtDate, model.SignupOpensAtTime),
                    SignupClosesAt = GetUtc(model.EnableSignupClosesAt, model.SignupClosesAtDate, model.SignupClosesAtTime),
                    SignupHelp = model.SignupHelp,
                    RoleSignup = model.RoleSignup,
                    RoleSignupHelp = model.RoleSignupHelp,
                    AllowPartnerSignup = model.AllowPartnerSignup,
                    AllowPartnerSignupHelp = model.AllowPartnerSignupHelp,
                    AutoAcceptedSignups = model.AutoAcceptedSignups
                }
            };

        public static async Task<EventModel> GetEventModel(this MemberContext context, Guid id)
        {
            var model = await context.Events
                .Include(e => e.SignupOptions)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.User)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.AuditLog)
                        .ThenInclude(l => l.User)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.Partner)
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == id);

            if (model == null) return null;

            return EventModel.Create(model);
        }

        public static IEnumerable<Guid> GetSelected(this EventSaveModel input)
            => input.Leads
                .Concat(input.Follows)
                .Concat(input.Solos)
                .Where(l => l.Selected)
                .Select(l => l.Id);

        public static async Task<EventInputModel> GetEventInputModel(this MemberContext context, Guid id)
        {
            var model = await context.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == id);

            if (model == null) return null;

            var (signupOpensAtDate, signupOpensAtTime) = GetLocal(model.SignupOptions.SignupOpensAt);
            var (signupClosesAtDate, signupClosesAtTime) = GetLocal(model.SignupOptions.SignupClosesAt);

            return new EventInputModel
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                EnableSignupOpensAt = model.SignupOptions.SignupOpensAt.HasValue,
                SignupOpensAtDate = signupOpensAtDate,
                SignupOpensAtTime = signupOpensAtTime,
                EnableSignupClosesAt = model.SignupOptions.SignupClosesAt.HasValue,
                SignupClosesAtDate = signupClosesAtDate,
                SignupClosesAtTime = signupClosesAtTime,
                PriceForMembers = model.SignupOptions.PriceForMembers,
                PriceForNonMembers = model.SignupOptions.PriceForNonMembers,
                RequiresMembershipFee = model.SignupOptions.RequiresMembershipFee,
                RequiresTrainingFee = model.SignupOptions.RequiresTrainingFee,
                RequiresClassesFee = model.SignupOptions.RequiresClassesFee,
                SignupHelp = model.SignupOptions.SignupHelp,
                RoleSignup = model.SignupOptions.RoleSignup,
                RoleSignupHelp = model.SignupOptions.RoleSignupHelp,
                AllowPartnerSignup = model.SignupOptions.AllowPartnerSignup,
                AllowPartnerSignupHelp = model.SignupOptions.AllowPartnerSignupHelp,
                AutoAcceptedSignups = model.SignupOptions.AutoAcceptedSignups
            };
        }

        public static void UpdateEvent(this Data.Event entity, EventInputModel model)
        {
            entity.Title = model.Title;
            entity.Description = model.Description;
            entity.Type = model.Type;
            entity.SignupOptions.RequiresMembershipFee = model.RequiresMembershipFee;
            entity.SignupOptions.RequiresTrainingFee = model.RequiresTrainingFee;
            entity.SignupOptions.RequiresClassesFee = model.RequiresClassesFee;
            entity.SignupOptions.PriceForMembers = model.PriceForMembers;
            entity.SignupOptions.PriceForNonMembers = model.PriceForNonMembers;
            entity.SignupOptions.SignupOpensAt = GetUtc(model.EnableSignupOpensAt, model.SignupOpensAtDate, model.SignupOpensAtTime);
            entity.SignupOptions.SignupClosesAt = GetUtc(model.EnableSignupClosesAt, model.SignupClosesAtDate, model.SignupClosesAtTime);
            entity.SignupOptions.SignupHelp = model.SignupHelp;
            entity.SignupOptions.RoleSignup = model.RoleSignup;
            entity.SignupOptions.RoleSignupHelp = model.RoleSignupHelp;
            entity.SignupOptions.AllowPartnerSignup = model.AllowPartnerSignup;
            entity.SignupOptions.AllowPartnerSignupHelp = model.AllowPartnerSignupHelp;
            entity.SignupOptions.AutoAcceptedSignups = model.AutoAcceptedSignups;
        }

        public static void SetEventStatus(this Data.Event model, string status)
        {
            if (status == "open")
            {
                model.SignupOptions.SignupOpensAt = TimeProvider.UtcNow;
                model.SignupOptions.SignupClosesAt = null;
            }
            else if (status == "close")
            {
                model.SignupOptions.SignupClosesAt = TimeProvider.UtcNow;
            }
            else if (status == "archive")
            {
                model.SignupOptions.SignupClosesAt = model.SignupOptions.SignupClosesAt ?? TimeProvider.UtcNow;
                model.Archived = true;
            }
        }

        public static Task EditEvent(this MemberContext context, Guid id, Action<Data.Event> action)
            => context.EditEvent(id, e =>
            {
                action(e);
                return Task.CompletedTask;
            });

        public static async Task EditEvent(this MemberContext context, Guid id, Func<Data.Event, Task> action)
        {
            var entry = await context.Events
                .Include(e => e.SignupOptions)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.User)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.AuditLog)
                .SingleOrDefaultAsync(e => e.Id == id);

            if (entry == null) return;

            await action(entry);

            await context.SaveChangesAsync();
        }

        internal static DateTime? GetUtc(bool enable, string date, string time)
        {
            if (!enable) return null;

            var dateTime = $"{date}T{time}:00";

            var localDateTime = LocalDateTimePattern.GeneralIso.Parse(dateTime).GetValueOrThrow();

            return localDateTime.InZoneLeniently(TimeProvider.TimeZoneOslo).ToDateTimeUtc();
        }

        internal static (string Date, string Time) GetLocal(DateTime? utc)
        {
            if (!utc.HasValue) return (null, null);

            var result = utc.Value.ToOsloZone();

            var date = result.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var time = result.TimeOfDay.ToString("HH:mm", CultureInfo.InvariantCulture);

            return (date, time);
        }
    }
}