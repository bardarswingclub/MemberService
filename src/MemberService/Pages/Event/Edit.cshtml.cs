namespace MemberService.Pages.Event;

using MemberService.Auth;
using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanEditEvent))]
public class EditModel : EventInputModel
{
    private readonly MemberContext _database;

    public EditModel(MemberContext database)
    {
        _database = database;
    }

    [BindProperty]
    public Guid Id { get; set; }

    public bool IsArchived { get; set; }

    public bool IsCancelled { get; set; }

    public async Task<IActionResult> OnGet(Guid id)
    {
        var model = await _database.Events
            .Include(e => e.SignupOptions)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (model == null) return NotFound();

        var (signupOpensAtDate, signupOpensAtTime) = model.SignupOptions.SignupOpensAt.GetLocalDateAndTime();
        var (signupClosesAtDate, signupClosesAtTime) = model.SignupOptions.SignupClosesAt.GetLocalDateAndTime();

        Id = model.Id;
        SemesterId = model.SemesterId;
        Title = model.Title;
        Description = model.Description;
        Type = model.Type;
        IsArchived = model.Archived;
        IsCancelled = model.Cancelled;
        EnableSignupOpensAt = model.SignupOptions.SignupOpensAt.HasValue;
        SignupOpensAtDate = signupOpensAtDate;
        SignupOpensAtTime = signupOpensAtTime;
        EnableSignupClosesAt = model.SignupOptions.SignupClosesAt.HasValue;
        SignupClosesAtDate = signupClosesAtDate;
        SignupClosesAtTime = signupClosesAtTime;
        PriceForMembers = model.SignupOptions.PriceForMembers;
        PriceForNonMembers = model.SignupOptions.PriceForNonMembers;
        RequiresMembershipFee = model.SignupOptions.RequiresMembershipFee;
        RequiresTrainingFee = model.SignupOptions.RequiresTrainingFee;
        RequiresClassesFee = model.SignupOptions.RequiresClassesFee;
        IncludedInTrainingFee = model.SignupOptions.IncludedInTrainingFee;
        IncludedInClassesFee = model.SignupOptions.IncludedInClassesFee;
        SignupHelp = model.SignupOptions.SignupHelp;
        RoleSignup = model.SignupOptions.RoleSignup;
        RoleSignupHelp = model.SignupOptions.RoleSignupHelp;
        AllowPartnerSignup = model.SignupOptions.AllowPartnerSignup;
        AllowPartnerSignupHelp = model.SignupOptions.AllowPartnerSignupHelp;
        AutoAcceptedSignups = model.SignupOptions.AutoAcceptedSignups;

        return Page();
    }

    public async Task<IActionResult> OnPost(Guid id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _database.EditEvent(id, e => e.UpdateEvent(this));

        return RedirectToAction(nameof(EventController.View), "Event", new { id });
    }
}
