using System;
using System.Collections.Generic;
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
using System.Text.Encodings.Web;

namespace MemberService.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<MemberUser> _signInManager;
        private readonly UserManager<MemberUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<MemberUser> signInManager,
            UserManager<MemberUser> userManager,
            IEmailSender emailSender,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

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

            var user = await GetOrCreateUser();

            var token = await _userManager.GenerateUserTokenAsync(user, "PasswordlessLoginProvider", "passwordless-auth");
            var callbackUrl = Url.Page(
                "/Account/LoginCallback",
                pageHandler: null,
                values: new { userId = user.Id, token },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.Email,
                "Logg inn - Bårdar Swing Club",
                CreateBody(callbackUrl, user));

            return RedirectToPage("/Account/LoginConfirmation");
        }

        private static string CreateBody(string callbackUrl, MemberUser user)
            => $@"{Greeting(user)}

            <p>
                For å logge inn hos Bårdar Swing Club, <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>trykk her</a>.
            </p>

            Hilsen<br>
            Bårdar Swing Club";

        private static string Greeting(MemberUser user)
            => string.IsNullOrEmpty(user.FullName)
                ? "Hei!"
                : $"Hei {user.FullName}!";

        private async Task<MemberUser> GetOrCreateUser()
        {
            if (await _userManager.FindByEmailAsync(Input.Email) is MemberUser user)
            {
                return user;
            }
            else
            {
                var newUser = new MemberUser { UserName = Input.Email, Email = Input.Email };
                var result = await _userManager.CreateAsync(newUser);
                if (result.Succeeded)
                {
                    return newUser;
                }
                else
                {
                    throw new Exception($"Couldn't create user, {result.Errors.Select(e => $"{e.Code}: {e.Description}").FirstOrDefault()}");
                }
            }
        }
    }
}
