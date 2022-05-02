namespace MemberService.Pages.Signup;
using System;
using System.Threading.Tasks;

using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class EditModel : PageModel
{
    private readonly MemberContext _database;

    public EditModel(MemberContext database)
    {
        _database = database;
    }

    public string Title { get; set; }

    public string Description { get; set; }

    public string SignupHelp { get; set; }

    public bool RoleSignup { get; set; }

    public string RoleSignupHelp { get; set; }

    public bool AllowPartnerSignup { get; set; }

    public string AllowPartnerSignupHelp { get; set; }

    public SignupInputModel Input { get; set; }

    public Guid? SurveyId { get; set; }

    public async Task<IActionResult> OnGet(Guid id, string redirectTo = null)
    {
        var model = await _database.Events
            .Include(e => e.Semester)
            .Include(e => e.SignupOptions)
            .Include(e => e.Signups.Where(s => s.CanEdit() && s.UserId == User.GetId()))
            .FirstOrDefaultAsync(e => e.Id == id);

        if (model is null) return NotFound();

        if (model.Archived || (model.Semester is Semester semester && !semester.IsActive()))
        {
            return RedirectToAction(nameof(SignupController.Event), "Signup", new { id, slug = model.Title.Slugify() });
        }

        if (model.Signups.FirstOrDefault() is not EventSignup signup)
        {
            return RedirectToAction(nameof(SignupController.Event), "Signup", new { id, slug = model.Title.Slugify() });
        }

        Title = model.Title;
        Description = model.Description;
        SignupHelp = model.SignupOptions.SignupHelp;
        RoleSignup = model.SignupOptions.RoleSignup;
        RoleSignupHelp = model.SignupOptions.RoleSignupHelp;
        AllowPartnerSignup = model.SignupOptions.AllowPartnerSignup;
        AllowPartnerSignupHelp = model.SignupOptions.AllowPartnerSignupHelp;
        SurveyId = model.SurveyId;
        Input = new()
        {
            Role = signup.Role,
            PartnerEmail = signup.PartnerEmail
        };

        return Page();
    }

    public async Task<IActionResult> OnPost(Guid id, [FromForm] SignupInputModel input, string redirectTo = null)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToPage(new { id });
        }

        var model = await _database.GetEditableEvent(id);

        if (model == null)
        {
            return NotFound();
        }

        var user = await _database.GetEditableUser(User.GetId());

        if (user.GetEditableEvent(id) is EventSignup eventSignup)
        {
            eventSignup.AuditLog.Add($"Changed signup\n\n{eventSignup.Role} -> {input.Role}\n\n{eventSignup.PartnerEmail} -> {input.PartnerEmail}", user);

            eventSignup.Role = input.Role;
            eventSignup.PartnerEmail = input.PartnerEmail?.Trim().Normalize().ToUpperInvariant();

            try
            {
                if (model.Survey != null)
                {
                    if (eventSignup.Response == null)
                    {
                        eventSignup.Response = new Response
                        {
                            Survey = model.Survey,
                            User = user
                        };
                    }

                    eventSignup.Response.Answers = model.Survey.Questions
                        .JoinWithAnswers(input.Answers)
                        .ToList();
                }
            }
            catch (ModelErrorException error)
            {
                ModelState.AddModelError(error.Key, error.Message);
                return RedirectToPage(new { id });
            }

            await _database.SaveChangesAsync();
        }

        if (redirectTo == null)
        {
            return RedirectToAction(nameof(SignupController.Event), "Signup", new { id });
        }
        else
        {
            return Redirect(redirectTo);
        }
    }
}
