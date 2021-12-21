namespace MemberService.Services;

using MemberService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;



using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

public class LoginService : ILoginService
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUrlHelper _urlHelper;

    public LoginService(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IUrlHelper urlHelper)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _urlHelper = urlHelper;
    }

    public bool IsLoggedIn(ClaimsPrincipal user)
    {
        return _signInManager.IsSignedIn(user);
    }

    public async Task<User> GetOrCreateUser(string email)
    {
        if (await _userManager.FindByEmailAsync(email) is User user)
        {
            return user;
        }
        else
        {
            var newUser = new User { UserName = email, Email = email };
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

    public async Task<string> LoginCode(User user)
    {
        return await _userManager.GenerateUserTokenAsync(user, "ShortToken", "passwordless-auth");
    }

    public async Task<string> LoginLink(User user, string returnUrl)
    {
        var token = await _userManager.GenerateUserTokenAsync(user, "LongToken", "passwordless-auth");

        return _urlHelper.ActionLink(
            "Index",
            "LoginCallback",
            new
            {
                userId = user.Id,
                token,
                returnUrl
            });
    }

    public async Task<IList<AuthenticationScheme>> GetExternalAuthenticationSchemes()
    {
        var result = await _signInManager.GetExternalAuthenticationSchemesAsync();

        return result.ToList();
    }
}
