namespace MemberService.Auth.Development;

using Microsoft.AspNetCore.Identity;

public class DummyShortTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser>
    where TUser : class
{
    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult(false);
    }

    public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
    {
        return Task.FromResult("");
    }

    public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
    {
#if DEBUG
        return Task.FromResult(true);
#else
            throw new System.Exception("Dummy methods are only available in debug builds!");
#endif
    }
}
