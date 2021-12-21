namespace MemberService.Pages.Signup;



public class ModelErrorException : Exception
{
    public string Key { get; }

    public ModelErrorException(string key, string message) : base(message)
    {
        Key = key;
    }
}
