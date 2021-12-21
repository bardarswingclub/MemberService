﻿namespace MemberService.Components.ExternalLogin;

using MemberService.Services;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

public class ExternalLoginViewComponent : ViewComponent
{
    private readonly ILoginService _loginService;

    public ExternalLoginViewComponent(ILoginService loginService)
    {
        _loginService = loginService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string returnUrl = null)
    {
        var model = new Model
        {
            ReturnUrl = returnUrl,
            ExternalLogins = await _loginService.GetExternalAuthenticationSchemes()
        };

        return View(model);
    }

    public class Model
    {
        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
    }
}
