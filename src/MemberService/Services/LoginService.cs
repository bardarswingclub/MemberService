using MemberService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MemberService.Services
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<MemberUser> _userManager;
        private readonly IUrlHelper _urlHelper;

        public LoginService(
            UserManager<MemberUser> userManager,
            IUrlHelper urlHelper)
        {
            _userManager = userManager;
            _urlHelper = urlHelper;
        }

        public async Task<MemberUser> GetOrCreateUser(string email)
        {
            if (await _userManager.FindByEmailAsync(email) is MemberUser user)
            {
                return user;
            }
            else
            {
                var newUser = new MemberUser { UserName = email, Email = email };
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

        public async Task<string> LoginLink(MemberUser user, string returnUrl)
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
    }
}
