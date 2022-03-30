namespace MemberService.Pages.Home;

using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Signup;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public SemesterModel Semester { get; set; }

    public EventsModel PartyModel { get; set; }

    public EventsModel WorkshopModel { get; set; }

    public EventsModel TrainingModel { get; set; }

    public async Task<IActionResult> OnGet(
        [FromServices] MemberContext database)
    {
        var userId = User.GetId();

        Semester = await database.Semesters
            .Current(s => new SemesterModel
            {
                Title = s.Title,
                SignupOpensAt = s.SignupOpensAt,
                AnyOpenSignups = s.Courses.Any(c => c.IsOpen()),
                Signups = s.Courses
                    .SelectMany(c => c.Signups)
                    .Where(es => es.UserId == userId)
                    .OrderBy(es => es.Priority)
                    .Select(es => CourseSignupModel.Create(es))
                    .ToList()
            });

        var openEvents = await database.GetEvents(userId, e => e.HasOpened());

        var futureEvents = await database.GetEvents(userId, e => e.WillOpen());

        PartyModel = new EventsModel
        {
            Title = "Fester",
            OpenEvents = openEvents.Where(e => e.Type == EventType.Party).ToList(),
            FutureEvents = futureEvents.Where(e => e.Type == EventType.Party).ToList()
        };

        WorkshopModel = new EventsModel
        {
            Title = "Workshops",
            OpenEvents = openEvents.Where(e => e.Type == EventType.Workshop).ToList(),
            FutureEvents = futureEvents.Where(e => e.Type == EventType.Workshop).ToList()
        };

        TrainingModel = new EventsModel
        {
            Title = "Egentrening",
            OpenEvents = openEvents.Where(e => e.Type == EventType.Training).ToList(),
            FutureEvents = futureEvents.Where(e => e.Type == EventType.Training).ToList()
        };

        return Page();
    }

    public class SemesterModel
    {
        public string Title { get; set; }

        public DateTime SignupOpensAt { get; set; }

        public bool AnyOpenSignups { get; set; }

        public IReadOnlyCollection<CourseSignupModel> Signups { get; set; }
    }
}
