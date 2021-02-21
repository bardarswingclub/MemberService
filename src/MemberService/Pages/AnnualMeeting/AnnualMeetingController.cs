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
            var isMember = await _database.Users
                .Expressionify()
                .Where(u => u.Id == GetUserId())
                .AnyAsync(u => u.HasPayedMembershipThisYear());

            return View(new Model
            {
                IsMember = isMember
            });
        }

        private string GetUserId() => _userManager.GetUserId(User);
    }
}