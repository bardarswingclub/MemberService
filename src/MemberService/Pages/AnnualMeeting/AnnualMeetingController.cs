using System;
using System.Linq;
using System.Threading.Tasks;

using Clave.Expressionify;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Event;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NodaTime.Extensions;

namespace MemberService.Pages.AnnualMeeting
{
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
            var meetings = await _database.AnnualMeetings
                .Expressionify()
                .OrderBy(m => m.MeetingStartsAt)
                .ToListAsync();

            var meeting = meetings
                .FirstOrDefault(m => m.MeetingEndsAt > TimeProvider.UtcNow);

            if (meeting is null)
            {
                var pastMeeting = meetings
                    .OrderByDescending(m => m.MeetingEndsAt)
                    .FirstOrDefault();

                if (pastMeeting is null)
                {
                    return View("NoMeeting");
                }

                return View(new Model
                {
                    Id = pastMeeting.Id,
                    Title = pastMeeting.Title,
                    MeetingSummary = pastMeeting.MeetingSummary
                });
            }

            var isMember = await _database.Users
                .Expressionify()
                .Where(u => u.Id == GetUserId())
                .AnyAsync(u => u.HasPayedMembershipThisYear());

            return View(new Model
            {
                Id = meeting.Id,
                IsMember = isMember,
                Title = meeting.Title,
                MeetingInvitation = meeting.MeetingInvitation,
                MeetingInfo = meeting.MeetingInfo,
                MeetingStartsAt = meeting.MeetingStartsAt,
                HasStarted = meeting.MeetingStartsAt < TimeProvider.UtcNow
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
        public async Task<IActionResult> Edit(Guid id, [FromForm] AnnualMeetingInputModel input)
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

            model.MeetingStartsAt = input.MeetingStartsAtDate.GetUtc(input.MeetingStartsAtTime);
            model.MeetingEndsAt = input.MeetingEndsAtDate.GetUtc(input.MeetingEndsAtTime);
            model.Title = input.Title;
            model.MeetingInvitation = input.Invitation;
            model.MeetingInfo = input.Info;
            model.MeetingSummary = input.Summary;

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private string GetUserId() => _userManager.GetUserId(User);
    }
}