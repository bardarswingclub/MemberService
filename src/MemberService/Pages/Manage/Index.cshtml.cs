using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Clave.ExtensionMethods;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Manage
{
    [Authorize]
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<MemberUser> _userManager;
        private readonly MemberContext _memberContext;
        private readonly SignInManager<MemberUser> _signInManager;

        public IndexModel(
            UserManager<MemberUser> userManager,
            MemberContext memberContext,
            SignInManager<MemberUser> signInManager)
        {
            _userManager = userManager;
            _memberContext = memberContext;
            _signInManager = signInManager;
        }

        [DisplayName("E-post")]
        public string Email { get; set; }

        public IReadOnlyCollection<Payment> Payments { get; private set; }

        public IReadOnlyCollection<SignupModel> EventSignups { get; private set; }

        [TempData]
        public string SuccessMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DisplayName("Fullt navn")]
            public string FullName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var user = await _memberContext.Users
                .Include(x => x.Payments)
                .Include(x => x.EventSignups)
                    .ThenInclude(s => s.Event)
                        .ThenInclude(e => e.SignupOptions)
                .SingleUser(userId);

            if (user == null)
            {
                return base.NotFound($"Unable to load user with ID '{userId}'.");
            }

            Email = user.Email;

            Payments = user.Payments
                .OrderByDescending(p => p.PayedAtUtc)
                .ToList();

            EventSignups = user.EventSignups
                .OrderByDescending(s => s.SignedUpAt)
                .Select(SignupModel.Create)
                .ToReadOnlyCollection();

            Input = new InputModel
            {
                FullName = user.FullName
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            SuccessMessage = "Navnet ditt har blitt lagret :)";
            return RedirectToPage();
        }
    }
}
