using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MemberService.Auth
{
    public class PasswordlessLoginTokenProvider<TUser> : DataProtectorTokenProvider<TUser>
    where TUser : class
    {
        public PasswordlessLoginTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<PasswordlessLoginTokenProviderOptions> options)
            : base(dataProtectionProvider, options)
        {
        }
    }
}