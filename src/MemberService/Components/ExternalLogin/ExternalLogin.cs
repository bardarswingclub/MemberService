namespace MemberService.Components.ExternalLogin;

using MemberService.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

public class ExternalLogin : ViewComponent
{
    private readonly ILoginService _loginService;

    public ExternalLogin(ILoginService loginService)
    {
        _loginService = loginService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string returnUrl = null)
        => View(new Model(returnUrl, await _loginService.GetExternalAuthenticationSchemes()));

    public record Model(string ReturnUrl, IList<AuthenticationScheme> ExternalLogins);
}
