namespace MemberService.Pages.Event;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(nameof(Policy.CanCreateEvent))]
public class CreateModel : EventInputModel
{
    private readonly MemberContext _database;

    public CreateModel(MemberContext database)
    {
        _database = database;
    }

    public IActionResult OnGet(EventType type)
    {
        Type = type;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _database.Get(User);
        var entity = this.ToEntity(user);

        if (entity.Type == EventType.Party && User.CanCreateParty())
        {
            _database.Events.Add(entity);
        }
        else if (entity.Type == EventType.Workshop && User.CanCreateWorkshop())
        {
            _database.Events.Add(entity);
        }
        else
        {
            return Forbid();
        }

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(EventController.View), "Event", new { id = entity.Id });
    }
}
