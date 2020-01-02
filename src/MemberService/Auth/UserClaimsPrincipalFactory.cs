using System.Security.Claims;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MemberService.Auth
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, MemberRole>
    {
        public UserClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<MemberRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("FullName", user.FullName ?? ""));
            return identity;
        }
    }
}