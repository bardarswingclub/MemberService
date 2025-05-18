namespace MemberService.Pages.Account;

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MemberService.Services;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ILoginService _loginService;
    private readonly IEmailService _emailService;
    private readonly RecaptchaSettings _recaptchaSettings;

    public LoginModel(
        ILoginService loginService,
        IEmailService emailService,
        IOptions<RecaptchaSettings> recaptchaOptions)
    {
        _loginService = loginService;
        _emailService = emailService;
        _recaptchaSettings = recaptchaOptions.Value;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    public string SiteKey => _recaptchaSettings.SiteKey;
    
    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "E-post")]
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

        var response = Request.Form["g-recaptcha-response"];
        var secret = _recaptchaSettings.SecretKey;
        if (string.IsNullOrEmpty(response))
        {
            ModelState.AddModelError(string.Empty, "reCAPCHA key missing");
            return Page();
        }
        var isValid = await ValidateRecaptcha(response, secret);

        if (!isValid)
        {
            ModelState.AddModelError(string.Empty, "reCAPTCHA-feil");
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

    private async Task<bool> ValidateRecaptcha(string response, string secret)
    {
        using var client = new HttpClient();
        var result = await client.PostAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={response}",
            null);

        var json = await result.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, "RC:" + json);
        dynamic parsed = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        return parsed.success == true;
    }
}
