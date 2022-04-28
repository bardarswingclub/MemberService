namespace MemberService.Pages.Home;

using MemberService.Data;
using MemberService.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class FeesModel : PageModel
{
    public (FeeStatus Status, Fee Fee) MembershipFee { get; set; }

    public (FeeStatus Status, Fee Fee) TrainingFee { get; set; }

    public (FeeStatus Status, Fee Fee) ClassesFee { get; set; }

    public async Task<IActionResult> OnGet(
        [FromServices] MemberContext database)
    {
        var user = await database.Users
            .Include(x => x.Payments)
            .SingleUser(User);

        MembershipFee = user.GetMembershipFee();
        TrainingFee = user.GetTrainingFee();
        ClassesFee = user.GetClassesFee();

        return Page();
    }
}
