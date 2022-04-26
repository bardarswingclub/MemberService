namespace MemberService.Pages.Signup;




using System.Linq.Expressions;


using Clave.Expressionify;
using Clave.ExtensionMethods;

using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.EntityFrameworkCore;

public static partial class Logic
{
    public static async Task<IReadOnlyList<EventModel>> GetEvents(this MemberContext db, string userId, Expression<Func<Data.Event, bool>> predicate)
        => await db.Events
            .Include(e => e.SignupOptions)
            .AsNoTracking()
            .Expressionify()
            .Where(e => e.Type != EventType.Class)
            .Where(e => e.Archived == false)
            .Where(predicate)
            .OrderBy(e => e.SignupOptions.SignupOpensAt)
            .Select(e => EventModel.Create(e, userId))
            .ToListAsync();

    public static async Task<Data.Event> GetEditableEvent(this MemberContext db, Guid id)
        => await db.Events
            .Include(e => e.Signups)
                .ThenInclude(s => s.Response)
                    .ThenInclude(r => r.Answers)
            .Include(e => e.SignupOptions)
            .Include(e => e.Survey)
                .ThenInclude(s => s.Questions)
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(e => e.Id == id);

    public static async Task<User> GetUser(this MemberContext database, string userId)
        => await database.Users
            .Include(u => u.Payments)
            .Include(u => u.EventSignups)
                .ThenInclude(s => s.Response)
                    .ThenInclude(r => r.Answers)
            .Include(u => u.EventSignups)
                .ThenInclude(u => u.Payment)
            .AsNoTracking()
            .SingleUser(userId);

    public static async Task<User> GetEditableUser(this MemberContext database, string userId)
        => await database.Users
            .Include(u => u.Payments)
            .Include(u => u.EventSignups)
            .SingleUser(userId);

    public static EventSignup GetEditableEvent(this User user, Guid id)
        => user.EventSignups
            .Where(e => e.CanEdit())
            .FirstOrDefault(e => e.EventId == id);

    public static bool ShouldAutoAccept(this Data.Event model, DanceRole role)
        => model.SignupOptions.AutoAcceptedSignups > model.Signups.Count(s => s.Role == role);

    public static EventSignup AddEventSignup(this User user, Guid id, DanceRole role, string partnerEmail, bool autoAccept, int priority = 1)
    {
        var status = autoAccept ? Status.Approved : Status.Pending;
        var signup = new EventSignup
        {
            EventId = id,
            Priority = priority,
            Role = role,
            PartnerEmail = partnerEmail?.Trim().Normalize().ToUpperInvariant(),
            Status = status,
            SignedUpAt = TimeProvider.UtcNow,
            AuditLog =
                {
                    { $"Signed up as {role}{(partnerEmail is string ? $" with partner {partnerEmail}" : "")}, status is {status}", user }
                }
        };

        user.EventSignups.Add(signup);

        return signup;
    }

    public static IEnumerable<QuestionAnswer> JoinWithAnswers(this ICollection<Question> questions, IList<Answer> answers)
    {
        foreach (var (question, index) in questions.WithIndex())
        {
            var selectedAnswers = answers
                .Where(a => a.QuestionId == question.Id)
                .SelectMany(a => a.Selected, (_, optionId) => new QuestionAnswer
                {
                    OptionId = optionId,
                    AnsweredAt = TimeProvider.UtcNow,
                })
                .ToReadOnlyCollection();

            switch (question.Type)
            {
                case QuestionType.Radio when selectedAnswers.Count == 0:
                    throw new ModelErrorException($"Answers[{index}].Selected", "Velg et av alternativene");

                case QuestionType.Radio:
                    yield return selectedAnswers.FirstOrDefault();
                    break;

                case QuestionType.CheckBox:
                    foreach (var answer in selectedAnswers)
                    {
                        yield return answer;
                    }
                    break;
            }
        }
    }

    [Expressionify]
    public static SignupRequirement GetRequirement(this User user, EventSignupOptions options)
        => user.MustPayClassesFee(options)
        ? SignupRequirement.MustPayClassesFee
        : user.MustPayTrainingFee(options)
        ? SignupRequirement.MustPayTrainingFee
        : user.MustPayMembershipFee(options)
        ? SignupRequirement.MustPayMembershipFee
        : user.MustPayMembersPrice(options)
        ? SignupRequirement.MustPayMembersPrice
        : user.MustPayNonMembersPrice(options)
        ? SignupRequirement.MustPayNonMembersPrice
        : SignupRequirement.None;

    [Expressionify]
    public static bool CanEdit(this EventSignup e)
        => e.Status == Status.Pending || e.Status == Status.Recommended || e.Status == Status.WaitingList;

    [Expressionify]
    public static bool MustPayNonMembersPrice(this User user, EventSignupOptions options)
        => options.PriceForNonMembers > 0 && !user.HasPayedMembershipThisYear()
                                          && !(options.IncludedInClassesFee && user.HasPayedClassesFeeThisSemester())
                                          && !(options.IncludedInTrainingFee && user.HasPayedTrainingFeeThisSemester());

    [Expressionify]
    public static bool MustPayMembersPrice(this User user, EventSignupOptions options)
        => options.PriceForMembers > 0 && user.HasPayedMembershipThisYear()
                                       && !(options.IncludedInClassesFee && user.HasPayedClassesFeeThisSemester())
                                       && !(options.IncludedInTrainingFee && user.HasPayedTrainingFeeThisSemester());

    [Expressionify]
    public static bool MustPayMembershipFee(this User user, EventSignupOptions options)
        => options.RequiresMembershipFee && !user.HasPayedMembershipThisYear();

    [Expressionify]
    public static bool MustPayTrainingFee(this User user, EventSignupOptions options)
        => options.RequiresTrainingFee && !user.HasPayedTrainingFeeThisSemester() && !user.ExemptFromTrainingFee;

    [Expressionify]
    public static bool MustPayClassesFee(this User user, EventSignupOptions options)
        => options.RequiresClassesFee && !user.HasPayedClassesFeeThisSemester() && !user.ExemptFromClassesFee;
}
