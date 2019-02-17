using System;
using System.Collections.Generic;
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

        public LoginCallbackModel(
            UserManager<MemberUser> userManager,
            SignInManager<MemberUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            if (_signInManager.IsSignedIn(User) || userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var isValid = await _userManager.VerifyUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth", token);

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
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
            }

            await _signInManager.SignInAsync(user, true, IdentityConstants.ApplicationScheme);

            return RedirectToPage("/Account/Manage/Index");
        }
    }
}
