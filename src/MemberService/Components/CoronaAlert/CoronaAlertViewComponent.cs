using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MemberService.Components.CoronaAlert
{
    public class CoronaAlertViewComponent : ViewComponent
    {

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View();
            }

            return View();
        }
    }
}