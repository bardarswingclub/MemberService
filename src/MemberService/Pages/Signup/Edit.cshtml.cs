namespace MemberService.Pages.Signup;
using System;
using System.Threading.Tasks;

using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize]
public class EditModel : PageModel
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;

    public EditModel(
        MemberContext database,
        UserManager<User> userManager)
    {
        _database = database;
        _userManager = userManager;
    }

    public SignupModel SignupModel { get; set; }

    public async Task<IActionResult> OnGet(Guid id, string redirectTo = null)
    {
        var model = await _database.GetSignupModel(id);

        if (model == null)
        {
            return NotFound();
        }

        model.User = await _database.GetUser(_userManager.GetUserId(User));

        if (model.User.GetEditableEvent(id) is not EventSignup eventSignup)
        {
            return RedirectToAction(nameof(SignupController.Event), "Signup", new { id, slug = model.Title.Slugify() });
        }

        model.UserEventSignup = eventSignup;
        model.Input = new SignupInputModel
        {
            Role = eventSignup.Role,
            PartnerEmail = eventSignup.PartnerEmail
        };

        SignupModel = model;

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

        var user = await _database.GetEditableUser(_userManager.GetUserId(User));

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
