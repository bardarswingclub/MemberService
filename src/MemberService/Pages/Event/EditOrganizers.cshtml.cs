namespace MemberService.Pages.Event;

using Clave.Expressionify;

using MemberService.Auth;
using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanEditEventOrganizers))]
public class EditOrganizersModel : PageModel
{
    private readonly MemberContext _database;

    public EditOrganizersModel(MemberContext database)
    {
        _database = database;
    }

    public Guid EventId { get; set; }

    public Guid? SemesterId { get; set; }

    public string EventTitle { get; set; }

    public IReadOnlyList<Organizer> Organizers { get; set; }

    [BindProperty]
    public string UserId { get; init; }

    [BindProperty]
    public bool CanEdit { get; init; }

    [BindProperty]
    public bool CanEditSignup { get; set; }

    [BindProperty]
    public bool CanSetSignupStatus { get; set; }

    [BindProperty]
    public bool CanEditOrganizers { get; set; }

    [BindProperty]
    public bool CanSetPresence { get; set; }

    [BindProperty]
    public bool CanAddPresenceLesson { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        var model = await _database.Events.FindAsync(id);

        if (model is null) return NotFound();

        EventId = model.Id;
        EventTitle = model.Title;
        SemesterId = model.SemesterId;

        Organizers = await _database.EventOrganizers
            .Where(o => o.EventId == id)
            .Select(o => new Organizer
            {
                Id = o.UserId,
                FullName = o.User.FullName,
                Email = o.User.Email,
                CanEdit = o.CanEdit,
                CanAddPresenceLesson = o.CanAddPresenceLesson,
                CanEditOrganizers = o.CanEditOrganizers,
                CanEditSignup = o.CanEditSignup,
                CanSetPresence = o.CanSetPresence,
                CanSetSignupStatus = o.CanSetSignupStatus
            })
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnGetUsers(Guid id, string q)
    {
        var model = await _database.Users
            .Expressionify()
            .Except(_database.EventOrganizers
                .Where(o => o.EventId == id)
                .Select(o => o.User))
            .Where(u => u.NameMatches(q))
            .Select(u => new
            {
                value = u.Id,
                text = u.FullName + " (" + u.Email + ")"
            })
            .Take(10)
            .ToListAsync();

        return new JsonResult(model);
    }

    public async Task<IActionResult> OnPostAdd(Guid id)
    {
        var currentUser = await _database.Users.SingleUser(User.GetId());

        _database.EventOrganizers.Add(new EventOrganizer
        {
            UserId = UserId,
            EventId = id,
            UpdatedAt = DateTime.UtcNow,
            UpdatedByUser = currentUser,
            CanEdit = CanEdit,
            CanEditSignup = CanEditSignup,
            CanSetSignupStatus = CanSetSignupStatus,
            CanEditOrganizers = CanEditOrganizers,
            CanSetPresence = CanSetPresence,
            CanAddPresenceLesson = CanAddPresenceLesson,
        });

        await _database.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostEdit(Guid id)
    {
        var organizer = await _database.EventOrganizers
            .FindAsync(id, UserId);

        if (organizer != null)
        {
            var currentUser = await _database.Users.SingleUser(User.GetId());
            organizer.UpdatedAt = DateTime.UtcNow;
            organizer.UpdatedByUser = currentUser;
            organizer.CanEdit = CanEdit;
            organizer.CanEditSignup = CanEditSignup;
            organizer.CanSetSignupStatus = CanSetSignupStatus;
            organizer.CanEditOrganizers = CanEditOrganizers;
            organizer.CanSetPresence = CanSetPresence;
            organizer.CanAddPresenceLesson = CanAddPresenceLesson;

            await _database.SaveChangesAsync();
        }

        return new OkResult();
    }

    public async Task<IActionResult> OnPostRemove(Guid id)
    {
        var organizer = await _database.EventOrganizers
            .FindAsync(id, UserId);

        if (organizer != null)
        {
            _database.EventOrganizers.Remove(organizer);

            await _database.SaveChangesAsync();
        }

        return RedirectToPage(new { id });
    }

    public class Organizer
    {
        public string Id { get; set; }

        public object FullName { get; set; }

        public object Email { get; set; }

        public bool CanEdit { get; set; }

        public bool CanEditSignup { get; set; }

        public bool CanSetSignupStatus { get; set; }

        public bool CanEditOrganizers { get; set; }

        public bool CanSetPresence { get; set; }

        public bool CanAddPresenceLesson { get; set; }
    }
}
