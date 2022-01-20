namespace MemberService.Pages.Event;

using Clave.Expressionify;

using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.EntityFrameworkCore;

using NodaTime.Text;

public static class Logic
{
    public static Task<List<EventEntry>> GetEvents(this MemberContext context, string userId, bool archived)
        => context.Events
            .AsNoTracking()
            .Expressionify()
            .Where(e => archived || e.Archived == false)
            .Where(e => e.SemesterId == null)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => EventEntry.Create(e, userId))
            .ToListAsync();

    public static Data.Event ToEntity(this EventInputModel model, User user)
        => new()
        {
            Title = model.Title,
            Description = model.Description,
            Type = model.SemesterId.HasValue ? EventType.Class : model.Type,
            CreatedAt = TimeProvider.UtcNow,
            CreatedByUser = user,
            SignupOptions = new()
            {
                RequiresMembershipFee = model.RequiresMembershipFee,
                RequiresTrainingFee = model.RequiresTrainingFee,
                RequiresClassesFee = model.RequiresClassesFee,
                PriceForMembers = model.PriceForMembers,
                PriceForNonMembers = model.PriceForNonMembers,
                IncludedInTrainingFee = model.IncludedInTrainingFee,
                IncludedInClassesFee = model.IncludedInClassesFee,
                SignupOpensAt = model.EnableSignupOpensAt ? GetUtc(model.SignupOpensAtDate, model.SignupOpensAtTime) : null,
                SignupClosesAt = model.EnableSignupClosesAt ? GetUtc(model.SignupClosesAtDate, model.SignupClosesAtTime) : null,
                SignupHelp = model.SignupHelp,
                RoleSignup = model.RoleSignup,
                RoleSignupHelp = model.RoleSignupHelp,
                AllowPartnerSignup = model.AllowPartnerSignup,
                AllowPartnerSignupHelp = model.AllowPartnerSignupHelp,
                AutoAcceptedSignups = model.AutoAcceptedSignups
            },
            Organizers =
            {
                    new()
                    {
                        User = user,
                        UpdatedByUser = user,
                        UpdatedAt = TimeProvider.UtcNow,
                        CanEdit = true,
                        CanEditOrganizers = true,
                        CanSetSignupStatus = true,
                        CanSetPresence = true,
                        CanAddPresenceLesson = true
                    }
            }
        };

    public static async Task<EventModel> GetEventModel(
        this MemberContext context,
        Guid id,
        DateTime? signedUpBefore,
        int? priority,
        string name,
        bool excludeAcceptedElsewhere,
        bool excludeApprovedElsewhere,
        bool excludeRecommendedElsewhere,
        bool onlyDeniedElsewhere,
        bool onlyRejectedElsewhere,
        bool onlyWaitingListElsewhere)
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
            .AsNoTracking()
            .Expressionify()
            .Where(e => e.EventId == id)
            .Filter(signedUpBefore.HasValue, e => e.SignedUpAt < signedUpBefore)
            .Filter(priority.HasValue, e => e.Priority == priority)
            .Filter(!string.IsNullOrWhiteSpace(name), e => e.User.NameMatches(name))
            .Filter(excludeAcceptedElsewhere || excludeApprovedElsewhere || excludeRecommendedElsewhere, e => !e.User.EventSignups
                .Where(s => s.Event.SemesterId == model.SemesterId)
                .Where(s => s.EventId != e.EventId)
                .Any(s => (excludeAcceptedElsewhere && s.Status == Status.AcceptedAndPayed)
                          || (excludeApprovedElsewhere && s.Status == Status.Approved)
                          || (excludeRecommendedElsewhere && s.Status == Status.Recommended)))
            .Filter(onlyDeniedElsewhere || onlyRejectedElsewhere || onlyWaitingListElsewhere, e => e.User.EventSignups
                .Where(s => s.Event.SemesterId == model.SemesterId)
                .Any(s => s.EventId == e.EventId
                          || (onlyDeniedElsewhere && s.Status == Status.Denied)
                          || (onlyRejectedElsewhere && s.Status == Status.RejectedOrNotPayed)
                          || (onlyWaitingListElsewhere && s.Status == Status.WaitingList)))

            .Select(signup => new { signup, partner = context.Users.Include(u => u.EventSignups).FirstOrDefault(u => u.NormalizedEmail == signup.PartnerEmail) })
            .ToListAsync();

        return EventModel.Create(model, signups.Select(s => EventSignupModel.Create(s.signup, s.partner)).ToList());
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
            IsArchived = model.Archived,
            IsCancelled = model.Cancelled,
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
            IncludedInTrainingFee = model.SignupOptions.IncludedInTrainingFee,
            IncludedInClassesFee = model.SignupOptions.IncludedInClassesFee,
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
        entity.SignupOptions.IncludedInTrainingFee = model.IncludedInTrainingFee;
        entity.SignupOptions.IncludedInClassesFee = model.IncludedInClassesFee;
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
        else if (status == "cancel")
        {
            model.SignupOptions.SignupClosesAt ??= TimeProvider.UtcNow;
            model.Cancelled = true;
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

    public static async Task<Data.Event> CloneEvent(this MemberContext context, Guid id, User user)
    {
        var entry = await context.Events
            .Include(e => e.SignupOptions)
            .Include(e => e.Survey)
            .ThenInclude(s => s.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entry == null) return null;

        var signupOptions = entry.SignupOptions;
        var survey = entry.Survey;

        var newEntry = context.Events.Add(new Data.Event
        {
            Title = entry.Title + " copy",
            Description = entry.Description,
            Type = entry.Type,
            CreatedByUser = user,
            SemesterId = entry.SemesterId,
            CreatedAt = TimeProvider.UtcNow,
            SignupOptions = new EventSignupOptions
            {
                SignupOpensAt = signupOptions.SignupOpensAt,
                SignupClosesAt = signupOptions.SignupClosesAt,
                AllowPartnerSignup = signupOptions.AllowPartnerSignup,
                AllowPartnerSignupHelp = signupOptions.AllowPartnerSignupHelp,
                AutoAcceptedSignups = signupOptions.AutoAcceptedSignups,
                IncludedInClassesFee = signupOptions.IncludedInClassesFee,
                IncludedInTrainingFee = signupOptions.IncludedInTrainingFee,
                PriceForMembers = signupOptions.PriceForMembers,
                PriceForNonMembers = signupOptions.PriceForNonMembers,
                RequiresClassesFee = signupOptions.RequiresClassesFee,
                RequiresMembershipFee = signupOptions.RequiresMembershipFee,
                RequiresTrainingFee = signupOptions.RequiresTrainingFee,
                RoleSignup = signupOptions.RoleSignup,
                RoleSignupHelp = signupOptions.RoleSignupHelp,
                SignupHelp = signupOptions.SignupHelp
            },
            Survey = survey == null
                ? null
                : new Data.Survey
                {
                    Title = survey.Title,
                    Description = survey.Description,
                    Questions = survey.Questions.Select(q => new Question
                    {
                        Title = q.Title,
                        Description = q.Description,
                        Type = q.Type,
                        Order = q.Order,
                        Options = q.Options.Select(o => new QuestionOption
                        {
                            Title = o.Title,
                            Description = q.Description
                        }).ToList()
                    }).ToList()
                },
            Organizers =
                {
                    new()
                    {
                        User = user,
                        UpdatedByUser = user,
                        UpdatedAt = TimeProvider.UtcNow,
                        CanEdit = true,
                        CanEditOrganizers = true,
                        CanSetSignupStatus = true,
                        CanSetPresence = true,
                        CanAddPresenceLesson = true
                    }
                }
        });

        await context.SaveChangesAsync();

        return newEntry.Entity;
    }

    internal static DateTime GetUtc(this string date, string time)
    {
        var dateTime = $"{date}T{time}:00";

        var localDateTime = LocalDateTimePattern.GeneralIso.Parse(dateTime).GetValueOrThrow();

        return localDateTime.InZoneLeniently(TimeProvider.TimeZoneOslo).ToDateTimeUtc();
    }

    internal static DateTime? GetUtcMaybe(this string date, string time)
    {
        if (date is null) return default;
        if (time is null) return default;
        return GetUtc(date, time);
    }
}
