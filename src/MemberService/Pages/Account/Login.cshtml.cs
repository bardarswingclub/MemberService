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
        private readonly IEmailService _emailService;

        public LoginModel(
            ILoginService loginService,
            IEmailService emailService)
        {
            _loginService = loginService;
            _emailService = emailService;
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
            if (_loginService.IsLoggedIn(User))
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

            await _emailService.SendLoginEmail(Input.Email, new Emails.Account.LoginModel
            {
                Name = user.FullName,
                CallbackUrl = await _loginService.LoginLink(user, Input.ReturnUrl),
                Code = await _loginService.LoginCode(user)
            });

            return RedirectToPage("/Account/LoginConfirmation", null, new { email = Input.Email, returnUrl = Input.ReturnUrl });
        }
    }
}
