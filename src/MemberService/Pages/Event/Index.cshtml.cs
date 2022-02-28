namespace MemberService.Pages.Event;

using System.Collections.Generic;

using MemberService.Auth;
using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(nameof(Policy.CanListEvents))]
public class IndexModel : PageModel
{
    private readonly MemberContext _database;

    public IndexModel(MemberContext database)
    {
        _database = database;
    }

    public List<EventEntry> Events { get; set; }

    public async Task<IActionResult> OnGet(bool archived = false)
    {
        Events = await _database.GetEvents(User.GetId(), archived);

        return Page();
    }
}
