namespace MemberService.Pages.Signup;
using Microsoft.Extensions.Options;
using MemberService.Services;

public class AnonymousModel
{

    public Guid Id { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }

    public RecaptchaSettings recaptchaSettings { get; init; }
    public string SiteKey => recaptchaSettings.SiteKey;
}
