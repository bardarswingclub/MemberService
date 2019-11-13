using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MemberService.Pages.Account
{
    [AllowAnonymous]
    public class LoginCallbackController : Controller
    {
        private readonly UserManager<MemberUser> _userManager;
        private readonly SignInManager<MemberUser> _signInManager;

        public LoginCallbackController(
            UserManager<MemberUser> userManager,
            SignInManager<MemberUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string userId, string token, string returnUrl)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (_signInManager.IsSignedIn(User))
            {
                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
            }

            var user = await _userManager.FindByIdAsync(userId);

            return await SignIn(user, token, "LongToken", returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Index(InputModel Input)
        {
            if (_signInManager.IsSignedIn(User) || !ModelState.IsValid)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            return await SignIn(user, Input.Code.Trim(), "ShortToken", Input.ReturnUrl);
        }

        private async Task<IActionResult> SignIn(MemberUser user, string token, string tokenType, string returnUrl)
        {
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var isValid = await _userManager.VerifyUserTokenAsync(user, tokenType, "passwordless-auth", token);

            if (!isValid)
            {
                return RedirectToPage("/Account/LoginConfirmation", new { user.Email, returnUrl, showError = true });
            }

            await _userManager.UpdateSecurityStampAsync(user);

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                await _signInManager.SignInAsync(user, true, IdentityConstants.ApplicationScheme);

                if (string.IsNullOrWhiteSpace(user.FullName))
                {
                    return RedirectToPage("/Account/Register", new { returnUrl });
                }

                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _userManager.ConfirmEmailAsync(user, code);

            await _signInManager.SignInAsync(user, true, IdentityConstants.ApplicationScheme);

            return RedirectToPage("/Account/Register", new { returnUrl });
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [DisplayName("E-post")]
            public string Email { get; set; }

            [Required]
            [DisplayName("Kode")]
            public string Code { get; set; }

            public string ReturnUrl { get; set; }
        }
    }
}
