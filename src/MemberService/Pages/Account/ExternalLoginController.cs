namespace MemberService.Pages.Account;

using System.Security.Claims;


using MemberService.Data;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[AllowAnonymous]
public class ExternalLoginController : Controller
{
    private readonly ILoginService _loginService;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ExternalLoginController> _logger;

    public ExternalLoginController(
        ILoginService loginService,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        ILogger<ExternalLoginController> logger)
    {
        _loginService = loginService;
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost]
    public IActionResult Index(string provider, string returnUrl = null)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Action(nameof(Callback), new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> Callback(string returnUrl = null, string remoteError = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        if (remoteError != null)
        {
            TempData.SetErrorMessage($"Error from external provider: {remoteError}");
            return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl });
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData.SetErrorMessage("Error loading external login information.");
            return RedirectToPage("/Account/Login", new { ReturnUrl = returnUrl });
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);

            var user = await _loginService.GetOrCreateUser(info.Principal.FindFirstValue(ClaimTypes.Email), info.Principal.FindFirstValue(ClaimTypes.Name));

            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                return RedirectToPage("/Account/Register", new { returnUrl });
            }

            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }
        else
        {
            var user = await _loginService.GetOrCreateUser(info.Principal.FindFirstValue(ClaimTypes.Email), info.Principal.FindFirstValue(ClaimTypes.Name));

            await _userManager.AddLoginAsync(user, info);

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                await _signInManager.SignInAsync(user, isPersistent: true, IdentityConstants.ExternalScheme);

                if (string.IsNullOrWhiteSpace(user.FullName))
                {
                    return RedirectToPage("/Account/Register", new { returnUrl });
                }

                return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _userManager.ConfirmEmailAsync(user, code);

            await _signInManager.SignInAsync(user, isPersistent: true, IdentityConstants.ExternalScheme);

            return RedirectToPage("/Account/Register", new { returnUrl });
        }
    }
}
