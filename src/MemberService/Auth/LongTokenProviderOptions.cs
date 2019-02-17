using System;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Auth
{
    public class LongTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public LongTokenProviderOptions()
        {
            Name = "LongTokenProvider";
            TokenLifespan = TimeSpan.FromDays(7);
        }
    }

}