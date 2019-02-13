using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MemberService.Data;
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
            var user = await GetCurrentUser();

            var thisYear = new DateTime(DateTime.Today.Year, 1, 1);
            var latestPayment = user.Payments
                .Where(p => p.PayedAt > thisYear)
                .FirstOrDefault();

            return View(new IndexViewModel
            {
                User = user,
                HasPayedThisYear = latestPayment != null
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay()
        {
            var user = await GetCurrentUser();
            _memberContext.Payments.Add(new Payment
            {
                User = user,
                PayedAt = DateTime.Now
            });
            await _memberContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<MemberUser> GetCurrentUser()
            => await _memberContext.GetUser(_userManager.GetUserId(User));
    }
}
