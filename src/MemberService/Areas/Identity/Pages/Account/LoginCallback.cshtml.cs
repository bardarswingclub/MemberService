using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemberService.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginCallbackModel : PageModel
    {
        private readonly UserManager<MemberUser> _userManager;
        private readonly SignInManager<MemberUser> _signInManager;

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

            public string ReturnUrl { get; set; }
        }

        public LoginCallbackModel(
            UserManager<MemberUser> userManager,
            SignInManager<MemberUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string token, string returnUrl)
        {
            if (_signInManager.IsSignedIn(User) || userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isValid = await _userManager.VerifyUserTokenAsync(user, "LongToken", "passwordless-auth", token);

            return await SignIn(user, isValid, returnUrl);
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

            return await SignIn(user, isValid, Input.ReturnUrl);
        }

        private async Task<IActionResult> SignIn(MemberUser user, bool isValid, string returnUrl)
        {
            if (!isValid)
            {
                return RedirectToPage("LoginConfirmation", new { user.Email, returnUrl, showError = true });
            }

            await _userManager.UpdateSecurityStampAsync(user);

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                await _signInManager.SignInAsync(user, true, IdentityConstants.ApplicationScheme);

                return Url.IsLocalUrl(returnUrl)
                    ? Redirect(returnUrl)
                    : Redirect("/");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _userManager.ConfirmEmailAsync(user, code);

            await _signInManager.SignInAsync(user, true, IdentityConstants.ApplicationScheme);

            return RedirectToPage("Register", new { returnUrl });
        }
    }
}
