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
            var isAdmin = User.IsInRole(Roles.ADMIN);

            var meetings = await _database.AnnualMeetings
                .Include(m => m.Attendees.Where(a => isAdmin))
                .ThenInclude(a => a.User)
                .Expressionify()
                .OrderBy(m => m.MeetingStartsAt)
                .ToListAsync();

            var meeting = meetings
                .FirstOrDefault(m => m.MeetingEndsAt > TimeProvider.UtcNow);

            var member = await _database.Users
                .Expressionify()
                .Where(u => u.Id == GetUserId())
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
            else if(meeting.IsLive())
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

            return View(new Model
            {
                Id = meeting.Id,
                IsMember = member != null,
                Title = meeting.Title,
                MeetingInvitation = meeting.MeetingInvitation,
                MeetingInfo = meeting.MeetingInfo,
                MeetingSummary = meeting.MeetingSummary,
                MeetingStartsAt = meeting.MeetingStartsAt,
                HasStarted = meeting.MeetingStartsAt < TimeProvider.UtcNow,
                HasEnded = meeting.MeetingEndsAt < TimeProvider.UtcNow,
                Attendees = meeting.Attendees.Select(a => new Model.Attendee
                {
                    UserId = a.UserId,
                    Name = a.User.FullName,
                    Visits = a.Visits,
                    FirstVisit = a.CreatedAt,
                    LastVisit = a.LastVisited
                }).ToList()
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

        private string GetUserId() => _userManager.GetUserId(User);
    }
}