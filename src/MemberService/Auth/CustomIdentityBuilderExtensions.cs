using MemberService.Auth.Development;
using Microsoft.AspNetCore.Identity;

namespace MemberService.Auth
{
    public static class CustomIdentityBuilderExtensions
    {
        public static IdentityBuilder AddPasswordlessLoginTokenProvider(this IdentityBuilder builder, bool isDevelopment)
        {
            builder.AddTokenProvider(
                "LongToken",
                typeof(LongTokenProvider<>).MakeGenericType(builder.UserType));

            builder.AddTokenProvider(
                "ShortToken",
                GetShortTokenProvider(isDevelopment).MakeGenericType(builder.UserType));

            return builder;
        }

        private static System.Type GetShortTokenProvider(bool useDummyProvider)
        {
            return useDummyProvider ? typeof(DummyShortTokenProvider<>) : typeof(ShortTokenProvider<>);
        }
    }

}