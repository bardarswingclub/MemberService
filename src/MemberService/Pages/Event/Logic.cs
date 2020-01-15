using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clave.Expressionify;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;

namespace MemberService.Pages.Event
{
    public static class Logic
    {
        public static Task<List<EventEntry>> GetEvents(this MemberContext context, bool archived)
            => context.Events
                .AsNoTracking()
                .Expressionify()
                .Where(e => archived || e.Archived == false)
                .Where(e => e.SemesterId == null)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => EventEntry.Create(e))
                .ToListAsync();

        public static Data.Event ToEntity(this EventInputModel model, User user)
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
                    SignupOpensAt = model.EnableSignupOpensAt ? GetUtc(model.SignupOpensAtDate, model.SignupOpensAtTime) : null,
                    SignupClosesAt = model.EnableSignupClosesAt ? GetUtc(model.SignupClosesAtDate, model.SignupClosesAtTime) : null,
                    SignupHelp = model.SignupHelp,
                    RoleSignup = model.RoleSignup,
                    RoleSignupHelp = model.RoleSignupHelp,
                    AllowPartnerSignup = model.AllowPartnerSignup,
                    AllowPartnerSignupHelp = model.AllowPartnerSignupHelp,
                    AutoAcceptedSignups = model.AutoAcceptedSignups
                }
            };

        public static async Task<EventModel> GetEventModel(
            this MemberContext context, 
            Guid id,
            DateTime? signedUpBefore,
            int? priority,
            string name,
            bool noOtherSpots)
        {
            var model = await context.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            
            if (model == null) return null;

            var signups = await context.EventSignups
                .Include(s => s.User)
                .Include(s => s.AuditLog)
                .ThenInclude(l => l.User)
                .Include(s => s.Partner)
                .ThenInclude(p => p.EventSignups)
                .AsNoTracking()
                .Expressionify()
                .Where(e => e.EventId == id)
                .Filter(signedUpBefore.HasValue, e => e.SignedUpAt < signedUpBefore)
                .Filter(priority.HasValue, e => e.Priority == priority)
                .Filter(!string.IsNullOrWhiteSpace(name), e => e.User.NameMatches(name))
                .Filter(noOtherSpots, e => !e.User.EventSignups.Where(s => s.Event.SemesterId == model.SemesterId).Where(s => s.EventId != e.EventId).Any(s => s.Status == Status.AcceptedAndPayed || s.Status == Status.Approved || s.Status == Status.Recommended))
                .ToListAsync();

            return EventModel.Create(model, signups);
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
                .FirstOrDefaultAsync(e => e.Id == id);

            if (model == null) return null;

            var (signupOpensAtDate, signupOpensAtTime) = model.SignupOptions.SignupOpensAt.GetLocalDateAndTime();
            var (signupClosesAtDate, signupClosesAtTime) = model.SignupOptions.SignupClosesAt.GetLocalDateAndTime();

            return new EventInputModel
            {
                Id = model.Id,
                SemesterId = model.SemesterId,
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
            entity.SignupOptions.SignupOpensAt = model.EnableSignupOpensAt ? GetUtc(model.SignupOpensAtDate, model.SignupOpensAtTime) : null;
            entity.SignupOptions.SignupClosesAt = model.EnableSignupClosesAt ? GetUtc(model.SignupClosesAtDate, model.SignupClosesAtTime) : null;
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
                model.SignupOptions.SignupClosesAt ??= TimeProvider.UtcNow;
                model.Archived = true;
            }
        }

        public static Task<Data.Event> EditEvent(this MemberContext context, Guid id, Action<Data.Event> action)
            => context.EditEvent(id, e =>
            {
                action(e);
                return Task.CompletedTask;
            });

        public static async Task<Data.Event> EditEvent(this MemberContext context, Guid id, Func<Data.Event, Task> action)
        {
            var entry = await context.Events
                .Include(e => e.SignupOptions)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.User)
                .Include(e => e.Signups)
                    .ThenInclude(s => s.AuditLog)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entry == null) return null;

            await action(entry);

            await context.SaveChangesAsync();

            return entry;
        }

        internal static DateTime? GetUtc(string date, string time)
        {
            var dateTime = $"{date}T{time}:00";

            var localDateTime = LocalDateTimePattern.GeneralIso.Parse(dateTime).GetValueOrThrow();

            return localDateTime.InZoneLeniently(TimeProvider.TimeZoneOslo).ToDateTimeUtc();
        }
    }
}