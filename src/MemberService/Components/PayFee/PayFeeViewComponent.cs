namespace MemberService.Components.PayFee;

using MemberService.Data;
using MemberService.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PayFeeViewComponent : ViewComponent
{
    private readonly MemberContext _database
        ;

    public PayFeeViewComponent(MemberContext database)
    {
        _database = database;
    }

    public async Task<IViewComponentResult> InvokeAsync(string type, string label, decimal? amount = null)
    {
        var user = await _database.Users
            .Include(x => x.Payments)
            .SingleUser(UserClaimsPrincipal);

        return View(new Model(type, label, amount ?? user.GetFee(type).Fee.Amount));
    };

    public record Model(string Type, string Label, decimal Amount);
}
