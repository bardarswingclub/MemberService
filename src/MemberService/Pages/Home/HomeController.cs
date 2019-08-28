using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Clave.ExtensionMethods;
using MemberService.Data;
using MemberService.Pages.Event;
using MemberService.Pages.Signup;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Home
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MemberContext _memberContext;
        private readonly UserManager<MemberUser> _userManager;

        public HomeController(
            MemberContext memberContext,
            UserManager<MemberUser> userManager)
        {
            _memberContext = memberContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var openEvents = await _memberContext.GetEvents(userId, e => e.IsOpen() && e.Type == EventType.Class);

            var futureEvents = await _memberContext.GetEvents(userId, e => e.WillOpen() && e.Type == EventType.Class);

            var willOpenAt = futureEvents
                .WhereNotNull(e => e.OpensAt)
                .OrderBy(e => e.OpensAt)
                .Select(e => e.OpensAt)
                .FirstOrDefault();

            return View(new HomeModel
            {
                Classes = openEvents,
                OpensAt = willOpenAt
            });
        }

        public async Task<IActionResult> Fees()
        {
            var user = await GetCurrentUser();

            return View(new FeesViewModel
            {
                MembershipFee = user.GetMembershipFee(),
                TrainingFee = user.GetTrainingFee(),
                ClassesFee = user.GetClassesFee()
            });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StatusCode(string statusCode)
        {
            return View();
        }

        private async Task<MemberUser> GetCurrentUser()
            => await _memberContext.Users
                .Include(x => x.Payments)
                .SingleUser(_userManager.GetUserId(User));
    }
}
