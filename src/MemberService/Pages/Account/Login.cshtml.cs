namespace MemberService.Pages.Account;

using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using MemberService.Services;

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

        var email = Input.Email.Trim();
        var user = await _loginService.GetOrCreateUser(email);

        await _emailService.SendLoginEmail(email, user.FullName, new Emails.Account.LoginModel
        {
            Name = user.FullName,
            CallbackUrl = await _loginService.LoginLink(user, Input.ReturnUrl),
            Code = await _loginService.LoginCode(user)
        });

        return RedirectToPage("/Account/LoginConfirmation", null, new { email, returnUrl = Input.ReturnUrl });
    }
}
