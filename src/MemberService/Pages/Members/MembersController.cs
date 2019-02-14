using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Members
{
    [Authorize(Roles = Roles.COORDINATOR_OR_ADMIN)]
    public class MembersController : Controller
    {
        private readonly MemberContext _memberContext;

        public MembersController(MemberContext memberContext)
        {
            _memberContext = memberContext;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _memberContext.Users
                .Include(u => u.Payments)
                .AsNoTracking()
                .ToListAsync();

            return View(users);
        }
    }
}