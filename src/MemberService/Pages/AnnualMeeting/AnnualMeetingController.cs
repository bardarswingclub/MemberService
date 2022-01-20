namespace MemberService.Pages.AnnualMeeting;

using Clave.Expressionify;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Pages.AnnualMeeting.Survey;
using MemberService.Pages.Event;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class AnnualMeetingController : Controller
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;

    public AnnualMeetingController(
        MemberContext database,
        UserManager<User> userManager)
    {
        _database = database;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var isAdmin = User.IsAdministrator();

        var meetings = await _database.AnnualMeetings
            .Include(m => m.Attendees.Where(a => isAdmin || a.UserId == userId))
            .ThenInclude(a => a.User)
            .Include(m => m.Survey)
            .Expressionify()
            .OrderBy(m => m.MeetingStartsAt)
            .ToListAsync();

        var meeting = meetings
            .FirstOrDefault(m => m.MeetingEndsAt > TimeProvider.UtcNow);

        var member = await _database.Users
            .Expressionify()
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync(u => u.HasPayedMembershipThisYear());

        if (meeting is null)
        {
            meeting = meetings
                .OrderByDescending(m => m.MeetingEndsAt)
                .FirstOrDefault();

            if (meeting is null)
            {
                return View("NoMeeting");
            }
        }
        else if (meeting.IsLive() && member is not null)
        {
            var attendee = meeting.Attendees.GetOrAdd(a => a.UserId == member.Id,
                () => new AnnualMeetingAttendee
                {
                    User = member,
                    CreatedAt = TimeProvider.UtcNow
                });

            attendee.Visits++;
            attendee.LastVisited = TimeProvider.UtcNow;

            await _database.SaveChangesAsync();
        }

        var results = await _database.AnnualMeetings
            .Expressionify()
            .Select(s => SurveyResultModel.Create(s))
            .FirstOrDefaultAsync(s => s.MeetingId == meeting.Id);

        return View(new Model
        {
            Id = meeting.Id,
            UserId = member?.Id,
            Title = meeting.Title,
            MeetingInvitation = meeting.MeetingInvitation,
            MeetingInfo = meeting.MeetingInfo,
            MeetingSummary = meeting.MeetingSummary,
            MeetingStartsAt = meeting.MeetingStartsAt,
            HasStarted = meeting.MeetingStartsAt < TimeProvider.UtcNow,
            HasEnded = meeting.MeetingEndsAt < TimeProvider.UtcNow,
            Attendees = meeting.Attendees.Where(a => a.User is not null).Select(a => new Model.Attendee
            {
                UserId = a.UserId,
                Name = a.User.FullName,
                Visits = a.Visits,
                FirstVisit = a.CreatedAt,
                LastVisit = a.LastVisited
            }).ToList(),
            VotingResults = results
        });
    }

    [HttpGet]
    [Authorize(nameof(Policy.IsAdmin))]
    public IActionResult Create()
    {
        var (startDate, startTime) = TimeProvider.UtcNow.GetLocalDateAndTime();

        return View(new AnnualMeetingInputModel
        {
            MeetingStartsAtDate = startDate,
            MeetingStartsAtTime = startTime,
            MeetingEndsAtDate = startDate,
            MeetingEndsAtTime = "23:59"
        });
    }

    [HttpPost]
    [Authorize(nameof(Policy.IsAdmin))]
    public async Task<IActionResult> Create([FromForm] AnnualMeetingInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        _database.AnnualMeetings.Add(new Data.AnnualMeeting
        {
            MeetingStartsAt = input.MeetingStartsAtDate.GetUtc(input.MeetingStartsAtTime),
            MeetingEndsAt = input.MeetingEndsAtDate.GetUtc(input.MeetingEndsAtTime),
            Title = input.Title,
            MeetingInvitation = input.Invitation,
            MeetingInfo = input.Info,
            MeetingSummary = input.Summary
        });

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(nameof(Policy.IsAdmin))]
    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await _database.AnnualMeetings.FirstOrDefaultAsync(m => m.Id == id);

        if (model is null)
        {
            return NotFound();
        }

        var (startDate, startTime) = model.MeetingStartsAt.GetLocalDateAndTime();
        var (endDate, endTime) = model.MeetingEndsAt.GetLocalDateAndTime();

        return View(new AnnualMeetingInputModel
        {
            Id = model.Id,
            Title = model.Title,
            Invitation = model.MeetingInvitation,
            Info = model.MeetingInfo,
            Summary = model.MeetingSummary,
            MeetingStartsAtDate = startDate,
            MeetingStartsAtTime = startTime,
            MeetingEndsAtDate = endDate,
            MeetingEndsAtTime = endTime
        });
    }

    [HttpPost]
    [Authorize(nameof(Policy.IsAdmin))]
    public async Task<IActionResult> Edit(Guid id, [FromForm] AnnualMeetingInputModel input, [FromForm] string submit)
    {
        if (!ModelState.IsValid)
        {
            input.Id = id;
            return View(input);
        }

        var model = await _database.AnnualMeetings.FirstOrDefaultAsync(m => m.Id == id);

        if (model is null)
        {
            return NotFound();
        }

        if (submit == "EndMeeting")
        {
            model.MeetingEndsAt = TimeProvider.UtcNow;
        }
        else
        {
            model.MeetingStartsAt = input.MeetingStartsAtDate.GetUtc(input.MeetingStartsAtTime);
            model.MeetingEndsAt = input.MeetingEndsAtDate.GetUtc(input.MeetingEndsAtTime);
            model.Title = input.Title;
            model.MeetingInvitation = input.Invitation;
            model.MeetingInfo = input.Info;
            model.MeetingSummary = input.Summary;

            model.Survey ??= new Data.Survey();
        }

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(nameof(Policy.IsAdmin))]
    public async Task<IActionResult> EndMeeting(Guid id)
    {
        var model = await _database.AnnualMeetings.FirstOrDefaultAsync(m => m.Id == id);

        if (model is null)
        {
            return NotFound();
        }

        model.MeetingEndsAt = TimeProvider.UtcNow;

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Vote(Guid id, [FromForm] Guid option)
    {
        var userId = GetUserId();

        var meeting = await _database.AnnualMeetings
            .Where(m => m.Id == id)
            .Select(m => new
            {
                SurveyId = m.SurveyId
            })
            .FirstOrDefaultAsync();

        if (meeting is null)
        {
            return NotFound();
        }

        var questionOption = await _database.QuestionOptions.FirstOrDefaultAsync(o => o.Id == option);

        var survey = await _database.Surveys
            .Include(s => s.Questions)
            .Include(s => s.Responses)
            .ThenInclude(r => r.Answers)
            .ThenInclude(a => a.Option)
            .FirstOrDefaultAsync(s => s.Id == meeting.SurveyId);

        var question = survey.Questions.FirstOrDefault(q => q.Id == questionOption.QuestionId);

        if (question.AnswerableFrom < TimeProvider.UtcNow && question.AnswerableUntil > TimeProvider.UtcNow)
        {
            var response = survey.Responses.GetOrAdd(r => r.UserId == GetUserId(), () => new Response { UserId = userId });

            response.Answers.RemoveWhere(a => a.Option.QuestionId == questionOption.QuestionId);

            response.Answers.Add(new QuestionAnswer { AnsweredAt = TimeProvider.UtcNow, OptionId = option });

            await _database.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetUserId() => _userManager.GetUserId(User);
}
