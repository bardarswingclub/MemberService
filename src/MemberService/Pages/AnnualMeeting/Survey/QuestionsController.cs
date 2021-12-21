namespace MemberService.Pages.AnnualMeeting.Survey;





using Clave.Expressionify;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.IsAdmin))]
[Route("/AnnualMeeting/{meetingId}/Questions/{action=Index}/{questionId?}")]
public class QuestionsController : Controller
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;

    public QuestionsController(
        MemberContext database,
        UserManager<User> userManager)
    {
        _database = database;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid meetingId)
    {
        var model = await _database
            .AnnualMeetings
            .Expressionify()
            .Where(s => s.SurveyId != null)
            .Select(s => SurveyResultModel.Create(s))
            .FirstOrDefaultAsync(s => s.MeetingId == meetingId);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid meetingId)
    {
        var model = await _database
            .AnnualMeetings
            .AsNoTracking()
            .Expressionify()
            .Where(s => s.SurveyId != null)
            .Select(s => SurveyModel.Create(s))
            .FirstOrDefaultAsync(s => s.MeetingId == meetingId);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(Guid meetingId, Guid questionId)
    {
        var model = await _database
            .AnnualMeetings
            .AsNoTracking()
            .Expressionify()
            .Where(m => m.SurveyId != null)
            .Where(m => m.Id == meetingId)
            .Select(m => QuestionModel.Create(meetingId, m.Survey.Questions.FirstOrDefault(q => q.Id == questionId)))
            .FirstOrDefaultAsync();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        Guid meetingId,
        [FromForm] QuestionType type)
    {
        var meeting = await _database.AnnualMeetings.FirstOrDefaultAsync(s => s.Id == meetingId);

        var model = await _database
            .Surveys
            .Include(s => s.Questions)
            .FirstOrDefaultAsync(e => e.Id == meeting.SurveyId);

        var question = new Question
        {
            Type = type,
            Order = model.Questions.Count
        };

        model.Questions.Add(question);

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(EditQuestion), new { meetingId, questionId = question.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Save(
        Guid meetingId,
        Guid questionId,
        QuestionInput input,
        [FromForm] string action)
    {
        var meeting = await _database.AnnualMeetings.FirstOrDefaultAsync(s => s.Id == meetingId);

        var question = await _database.Questions
            .Include(q => q.Options)
            .Where(q => q.SurveyId == meeting.SurveyId)
            .SingleAsync(q => q.Id == questionId);

        if (question == null)
        {
            return NotFound();
        }

        if (action == "save" || action == "add-option")
        {
            question.Title = input.Title;
            question.Description = input.Description;
            question.AnswerableFrom = TimeProvider.UtcNow.WithOsloTime(input.From);
            question.AnswerableUntil = TimeProvider.UtcNow.WithOsloTime(input.Until);

            foreach (var (o, option) in input.Options.Join(question.Options, o => o.Id, o => o.Id))
            {
                option.Title = o.Title;
                option.Description = o.Description;
                option.Order = input.Options.IndexOf(o);
            }
        }

        if (action == "delete")
        {
            _database.Questions.Remove(question);

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Index), "AnnualMeeting", new { meetingId });
        }

        if (action == "add-option")
        {
            question.Options.Add(new QuestionOption());
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            foreach (var (o, option) in input.Options.Join(question.Options, o => o.Id, o => o.Id))
            {
                option.Order = input.Options.IndexOf(o);

                if (o.Action == "delete")
                {
                    question.Options.Remove(option);
                }
            }
        }

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(EditQuestion), new { meetingId, questionId });
    }

    [HttpPost]
    public async Task<IActionResult> StartVoting(Guid meetingId, Guid questionId)
    {
        var meeting = await _database.AnnualMeetings.FirstOrDefaultAsync(s => s.Id == meetingId);

        var question = await _database.Questions
            .Include(q => q.Options)
            .Where(q => q.SurveyId == meeting.SurveyId)
            .SingleAsync(q => q.Id == questionId);

        if (question == null)
        {
            return NotFound();
        }

        question.AnswerableFrom = TimeProvider.UtcNow;
        question.AnswerableUntil ??= TimeProvider.UtcNow.AddMinutes(5);

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Index), "AnnualMeeting");
    }

    [HttpPost]
    public async Task<IActionResult> EndVoting(Guid meetingId, Guid questionId)
    {
        var meeting = await _database.AnnualMeetings.FirstOrDefaultAsync(s => s.Id == meetingId);

        var question = await _database.Questions
            .Include(q => q.Options)
            .Where(q => q.SurveyId == meeting.SurveyId)
            .SingleAsync(q => q.Id == questionId);

        if (question == null)
        {
            return NotFound();
        }

        question.AnswerableUntil = TimeProvider.UtcNow;

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Index), "AnnualMeeting");
    }
}
