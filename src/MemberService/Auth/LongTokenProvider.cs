namespace MemberService.Auth;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class LongTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
where TUser : class
{
    public LongTokenProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<LongTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
    {
    }
}
