using MemberService.Auth.Email;

namespace MemberService.Auth.Development
{
    public class DummySlowConsoleEmailSender : DummyConsoleEmailSender, ISlowEmailSender
    {

    }
}