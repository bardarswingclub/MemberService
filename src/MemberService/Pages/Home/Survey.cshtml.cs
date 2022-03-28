namespace MemberService.Pages.Home;
using System;

using Clave.Expressionify;

using MemberService.Data;
using MemberService.Pages.Signup;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class SurveyModel : PageModel
{
    public string Title { get; set; }

    public Guid SurveyId { get; set; }

    public async Task<IActionResult> OnGet(
        [FromServices] MemberContext database)
    {
        var model = await database.Semesters
            .Expressionify()
            .Where(s => s.IsActive())
            .Select(s => new
            {
                Title = s.Title,
                SurveyId = s.SurveyId,
            })
            .FirstOrDefaultAsync();

        if (model?.SurveyId is not Guid surveyId)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        Title = model.Title;
        SurveyId = surveyId;

        return Page();
    }

    public async Task<IActionResult> OnPost(
        [FromServices] MemberContext database,
        [FromForm] SurveyInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Survey));
        }

        var userId = User.GetId();

        var model = await database.Surveys
            .Include(s => s.Questions)
            .Include(s => s.Responses.Where(r => r.UserId == userId))
            .ThenInclude(r => r.Answers)
            .Expressionify()
            .Where(s => s.Semester.IsActive())
            .FirstOrDefaultAsync();

        if (model == null)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        var response = model.Responses.GetOrAdd(r => r.UserId == userId, () => new Response { UserId = userId });

        try
        {
            response.Answers = model.Questions
                .JoinWithAnswers(input.Answers)
                .ToList();
        }
        catch (ModelErrorException error)
        {
            ModelState.AddModelError(error.Key, error.Message);
            return RedirectToPage();
        }

        await database.SaveChangesAsync();

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}
