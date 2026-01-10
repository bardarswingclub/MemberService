using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace MemberService.Pages.Home
{
    [Authorize]
    public class SignupPageModel : PageModel
    {
        private readonly MemberContext _memberContext;
        private readonly UserManager<User> _userManager;

        public SignupPageModel(MemberContext memberContext, UserManager<User> userManager)
        {
            _memberContext = memberContext;
            _userManager = userManager;
        }

        [BindProperty]
        public SignupInputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var latestConsent = await _memberContext.SomeConsentRecords
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.ChangedAtUtc)
                .FirstOrDefaultAsync();

            Input.NewConsent.Options = Enum.GetValues<SomeConsentState>()
                .Select(s => new SelectListItem
                {
                    Text = s.GetDisplayName(),
                    Value = ((int)s).ToString(),
                    Selected = latestConsent?.State == s
                }).ToList();

            Input.NewConsent.SelectedState = latestConsent?.State;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var record = new SomeConsentRecord(Input.NewConsent.SelectedState!.Value, user.Id);
            _memberContext.SomeConsentRecords.Add(record);
            await _memberContext.SaveChangesAsync();

            // handle accept box etc.
            return RedirectToAction("Courses", "Home");
        }
    }
}
