namespace MemberService.Components.CoronaAlert;

using Microsoft.AspNetCore.Mvc;

public class CoronaAlertViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
