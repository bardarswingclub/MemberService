namespace MemberService.Pages.AnnualMeeting;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class NoMeetingModel : PageModel
{
    public IActionResult OnGet()
    {
        return Page();
    }
}
