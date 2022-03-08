namespace MemberService.Pages.Event;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(nameof(Policy.CanCreateSemesterEvent))]
public class CreateClassModel : EventInputModel
{
    private readonly MemberContext _database;

    public CreateClassModel(MemberContext database)
    {
        _database = database;
    }

    public async Task<IActionResult> OnGet()
    {
        var semester = await _database.Semesters.Current();

        var (date, time) = semester.SignupOpensAt.GetLocalDateAndTime();

        Type = EventType.Class;
        SemesterId = semester.Id;
        SignupOpensAtDate = date;
        SignupOpensAtTime = time;
        EnableSignupOpensAt = true;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _database.Users.SingleUser(User.GetId());
        var entity = this.ToEntity(user);

        var semester = await _database.Semesters.Current();

        semester.Courses.Add(entity);

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(EventController.View), "Event", new { id = entity.Id });
    }
}
