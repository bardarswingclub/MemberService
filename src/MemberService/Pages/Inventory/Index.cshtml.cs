namespace MemberService.Pages.Inventory;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

[Authorize(Policy = nameof(Policy.CanBorrowInventory))]
public class IndexModel : PageModel
{
    public bool CanManageInventory { get; private set; }

    public void OnGet()
    {
        CanManageInventory = User.IsInAnyRole(Roles.ADMIN, Roles.INVENTORY_MANAGER);
    }
}
