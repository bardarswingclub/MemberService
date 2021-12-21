namespace MemberService.Components.CoronaAlert;

using Microsoft.AspNetCore.Mvc;

public class CoronaAlertViewComponent : ViewComponent
{

    public async Task<IViewComponentResult> InvokeAsync()
    {
        await Task.CompletedTask;
        if (!User.Identity.IsAuthenticated)
        {
            return View();
        }

        return View();
    }
}
