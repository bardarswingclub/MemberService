namespace MemberService.Emails.Account;

public class LoginModel
{
    public string Name { get; set; }

    public string CallbackUrl { get; set; }

    public string Code { get; set; }
}
