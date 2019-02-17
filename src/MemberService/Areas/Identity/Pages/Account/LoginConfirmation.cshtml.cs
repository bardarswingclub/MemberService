using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemberService.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginConfirmationModel : PageModel
    {
        private readonly UserManager<MemberUser> _userManager;
        private readonly SignInManager<MemberUser> _signInManager;

        public LoginConfirmationModel(
            UserManager<MemberUser> userManager,
            SignInManager<MemberUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Email { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "E-post")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "Kode")]
            public string Code { get; set; }
        }

        public IActionResult OnGet(string email)
        {
            if (email == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Email = email;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (_signInManager.IsSignedIn(User) || !ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{Input.Email}'.");
            }

            var isValid = await _userManager.VerifyUserTokenAsync(user, "ShortToken", "passwordless-auth", Input.Code.Trim());

            if (!isValid)
            {
                return Page();
            }

            await _userManager.UpdateSecurityStampAsync(user);

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                await _signInManager.SignInAsync(user, true, IdentityConstants.ApplicationScheme);
                return RedirectToAction("Index", "Home");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _userManager.ConfirmEmailAsync(user, code);

            await _signInManager.SignInAsync(user, true, IdentityConstants.ApplicationScheme);

            return RedirectToPage("/Account/Manage/Index");
        }
    }
}
