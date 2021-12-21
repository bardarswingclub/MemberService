namespace MemberService.Auth;

using Microsoft.AspNetCore.Identity;

public class LongTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public LongTokenProviderOptions()
    {
        Name = "LongTokenProvider";
        TokenLifespan = TimeSpan.FromDays(7);
    }
}
