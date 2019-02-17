using Microsoft.AspNetCore.Identity;

namespace MemberService.Auth
{
    public static class CustomIdentityBuilderExtensions
    {
        public static IdentityBuilder AddPasswordlessLoginTokenProvider(this IdentityBuilder builder)
        {
            builder.AddTokenProvider(
                "LongToken",
                typeof(LongTokenProvider<>).MakeGenericType(builder.UserType));

            builder.AddTokenProvider(
                "ShortToken",
                typeof(ShortTokenProvider<>).MakeGenericType(builder.UserType));

            return builder;
        }
    }

}