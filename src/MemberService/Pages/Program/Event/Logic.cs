using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Clave.ExtensionMethods;
using MemberService.Data;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;

namespace MemberService.Pages.Program.Event
{
    public static class Logic
    {
        private static readonly Status[] Statuses = new[] {
            Status.AcceptedAndPayed,
            Status.Approved,
            Status.WaitingList,
            Status.Recommended,
            Status.Pending,
            Status.RejectedOrNotPayed,
            Status.Denied,
        };

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
                CreatedAt = DateTime.UtcNow,
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

            model.ConnectPartner();

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

            var (signupOpensAtDate, signupOpensAtTime) = model.SignupOptions.SignupOpensAt.GetLocalDateAndTime();
            var (signupClosesAtDate, signupClosesAtTime) = model.SignupOptions.SignupClosesAt.GetLocalDateAndTime();

            return new EventInputModel
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
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
                model.SignupOptions.SignupOpensAt = DateTime.UtcNow;
                model.SignupOptions.SignupClosesAt = null;
            }
            else if (status == "close")
            {
                model.SignupOptions.SignupClosesAt = DateTime.UtcNow;
            }
            else if (status == "archive")
            {
                model.SignupOptions.SignupClosesAt = model.SignupOptions.SignupClosesAt ?? DateTime.UtcNow;
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

        internal static void ConnectPartner(this Data.Event model)
            => model.Signups
                .Select(s => s.Partner)
                .WhereNotNull()
                .Join(model.Signups, p => p.Id, s => s.UserId)
                .ForEach(((MemberUser partner, EventSignup signup) x) => x.partner.EventSignups.Add(x.signup));

        internal static IReadOnlyList<EventSignupStatusModel> GroupByStatus(this ICollection<EventSignup> signups)
            => Statuses
                .Select(s => (s, signups.Where(x => x.Status == s)))
                .Select(EventSignupStatusModel.Create)
                .ToReadOnlyList();

        private static DateTime? GetUtc(bool enable, string date, string time)
        {
            if (!enable) return null;

            return LocalDateTimePattern.GeneralIso.Parse($"{date}T{time}:00")
                .GetValueOrThrow()
                .InZoneLeniently(Constants.TimeZoneOslo)
                .ToDateTimeUtc();
        }
    }
}