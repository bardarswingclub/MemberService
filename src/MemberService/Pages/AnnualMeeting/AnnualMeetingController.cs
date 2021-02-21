using System.Linq;
using System.Threading.Tasks;

using Clave.Expressionify;

using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            var liveMeeting = meetings
                .FirstOrDefault(m => m.MeetingEndsAt > TimeProvider.UtcNow);

            if (liveMeeting is null)
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
                IsMember = isMember,
                Title = liveMeeting.Title,
                MeetingInvitation = liveMeeting.MeetingInvitation,
                MeetingInfo = liveMeeting.MeetingInfo,
                MeetingStartsAt = liveMeeting.MeetingStartsAt
            });
        }

        private string GetUserId() => _userManager.GetUserId(User);
    }
}