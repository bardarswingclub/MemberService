using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MemberService.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel;
using System.Web;
using MemberService.Services;

namespace MemberService.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ILoginService _loginService;
        private readonly SignInManager<MemberUser> _signInManager;
        private readonly UserManager<MemberUser> _userManager;
        private readonly IEmailSender _emailSender;

        public LoginModel(
            ILoginService loginService,
            SignInManager<MemberUser> signInManager,
            UserManager<MemberUser> userManager,
            IEmailSender emailSender)
        {
            _loginService = loginService;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [DisplayName("E-post")]
            public string Email { get; set; }

            public string ReturnUrl { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl, string email = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            Input = new InputModel
            {
                ReturnUrl = returnUrl,
                Email = email
            };

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _loginService.GetOrCreateUser(Input.Email);

            var code = await _userManager.GenerateUserTokenAsync(user, "ShortToken", "passwordless-auth");

            await _emailSender.SendEmailAsync(
                Input.Email,
                $"Logg inn - {code} - Bårdar Swing Club",
                await CreateBody(user, code));

            return RedirectToPage("/Account/LoginConfirmation", null, new { email = Input.Email, returnUrl = Input.ReturnUrl });
        }

        private async Task<string> CreateBody(MemberUser user, string code)
        {
            string callbackUrl = await _loginService.LoginLink(user, Input.ReturnUrl);

            return $@"<h2>{Greeting(user)}</h2>

            <p>
                Bruk denne koden for å logge inn på siden
            </p>

            <h1>{code}</h1>

            <p>
                Eller <a href='{HttpUtility.HtmlAttributeEncode(callbackUrl)}'>trykk her</a>.
            </p>

            <i>Hilsen</i><br>
            <i>Bårdar Swing Club</i>";
        }

        private static string Greeting(MemberUser user)
            => string.IsNullOrEmpty(user.FullName)
                ? "Hei!"
                : $"Hei {user.FullName}!";
    }
}
