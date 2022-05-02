namespace MemberService.Pages.AnnualMeeting;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Pages.AnnualMeeting.Survey;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class IndexModel : PageModel
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;
    private readonly IAuthorizationService _authorizationService;

    public IndexModel(
        MemberContext database,
        UserManager<User> userManager,
        IAuthorizationService authorizationService)
    {
        _database = database;
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    public Guid Id { get; set; }

    public string Title { get; set; }

    public string MeetingInvitation { get; set; }

    public string MeetingInfo { get; set; }

    public string MeetingSummary { get; set; }

    public bool IsMember => UserId is not null;

    public DateTime MeetingStartsAt { get; set; }

    public bool HasStarted { get; set; }

    public bool HasEnded { get; set; }

    public IReadOnlyList<Attendee> Attendees { get; set; }

    public SurveyResultModel VotingResults { get; set; }

    public string UserId { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var userId = User.GetId();
        var canSeeAll = await _authorizationService.IsAuthorized(User, Policy.CanViewAnnualMeetingAttendees);

        var meetings = await _database.AnnualMeetings
            .Include(m => m.Attendees.Where(a => canSeeAll || a.UserId == userId))
            .ThenInclude(a => a.User)
            .Include(m => m.Survey)
            .OrderBy(m => m.MeetingStartsAt)
            .ToListAsync();

        var meeting = meetings
            .FirstOrDefault(m => m.MeetingEndsAt > TimeProvider.UtcNow);

        UserId = await _database.Users
            .Where(u => u.Id == userId)
            .Where(u => u.HasPayedMembershipThisYear())
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        if (meeting is null)
        {
            meeting = meetings
                .OrderByDescending(m => m.MeetingEndsAt)
                .FirstOrDefault();

            if (meeting is null)
            {
                return RedirectToPage("/AnnualMeeting/NoMeeting");
            }
        }
        else if (meeting.IsLive() && IsMember)
        {
            var attendee = meeting.Attendees.GetOrAdd(a => a.UserId == UserId,
                () => new()
                {
                    UserId = UserId,
                    CreatedAt = TimeProvider.UtcNow
                });

            attendee.Visits++;
            attendee.LastVisited = TimeProvider.UtcNow;

            await _database.SaveChangesAsync();
        }

        VotingResults = await _database.AnnualMeetings
            .Select(s => SurveyResultModel.Create(s))
            .FirstOrDefaultAsync(s => s.MeetingId == meeting.Id);

        Id = meeting.Id;
        Title = meeting.Title;
        MeetingInvitation = meeting.MeetingInvitation;
        MeetingInfo = meeting.MeetingInfo;
        MeetingSummary = meeting.MeetingSummary;
        MeetingStartsAt = meeting.MeetingStartsAt;
        HasStarted = meeting.MeetingStartsAt < TimeProvider.UtcNow;
        HasEnded = meeting.MeetingEndsAt < TimeProvider.UtcNow;
        Attendees = meeting.Attendees
            .WhereNotNull(a => a.User)
            .Select(a => new Attendee
            {
                UserId = a.UserId,
                Name = a.User.FullName,
                Visits = a.Visits,
                FirstVisit = a.CreatedAt,
                LastVisit = a.LastVisited
            })
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPost(Guid id, [FromForm] Guid option)
    {
        var userId = User.GetId();

        var surveyId = await _database.AnnualMeetings
            .Where(m => m.Id == id)
            .Select(m => m.SurveyId)
            .FirstOrDefaultAsync();

        if (surveyId is null)
        {
            return NotFound();
        }

        var questionOption = await _database.QuestionOptions.FindAsync(option);

        var survey = await _database.Surveys
            .Include(s => s.Questions)
            .Include(s => s.Responses)
            .ThenInclude(r => r.Answers)
            .ThenInclude(a => a.Option)
            .FirstOrDefaultAsync(s => s.Id == surveyId);

        var question = survey.Questions.FirstOrDefault(q => q.Id == questionOption.QuestionId);

        if (question.AnswerableFrom < TimeProvider.UtcNow && question.AnswerableUntil > TimeProvider.UtcNow)
        {
            var response = survey.Responses.GetOrAdd(r => r.UserId == userId, () => new() { UserId = userId });

            response.Answers.RemoveWhere(a => a.Option.QuestionId == questionOption.QuestionId);

            response.Answers.Add(new() { AnsweredAt = TimeProvider.UtcNow, OptionId = option });

            await _database.SaveChangesAsync();
        }

        return RedirectToPage(nameof(Index));
    }

    public class Attendee
    {
        public string UserId { get; set; }

        public string Name { get; set; }

        public DateTime FirstVisit { get; set; }

        public DateTime LastVisit { get; set; }

        public int Visits { get; set; }
    }
}
