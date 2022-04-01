namespace MemberService.Components.CoronaAlert;

using Microsoft.AspNetCore.Mvc;

public class CoronaAlert : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
