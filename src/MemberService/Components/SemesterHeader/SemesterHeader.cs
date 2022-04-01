namespace MemberService.Components.SemesterHeader;

using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;

public class SemesterHeader : ViewComponent
{
    private readonly MemberContext _database;

    public SemesterHeader(MemberContext database)
    {
        _database = database;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid? id)
    {
        if (id is null)
        {
            return new ContentViewComponentResult(string.Empty);
        }

        var model = await _database.Semesters
            .Select(e => new Model
            {
                Id = e.Id,
                Title = e.Title,
                IsActive = e.IsActive(),
                SurveyId = e.SurveyId,
                Events = e.Courses
                    .Where(c => !c.Archived)
                    .Select(c => new Model.Event
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Roles = c.SignupOptions.RoleSignup,
                        PendingLeads = c.Signups.Count(s => s.Status == Status.Pending && s.Role == DanceRole.Lead),
                        PendingFollows = c.Signups.Count(s => s.Status == Status.Pending && s.Role == DanceRole.Follow),
                        Pending = c.Signups.Count(s => s.Status == Status.Pending)
                    })
                    .OrderBy(c => c.Title)
                    .ToList()
            })
            .FirstOrDefaultAsync(e => e.Id == id.Value);

        return View(model);
    }

    public class Model
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
        public bool IsActive { get; init; }
        public Guid? SurveyId { get; init; }
        public IReadOnlyCollection<Event> Events { get; init; }

        public class Event
        {
            public Guid Id { get; init; }
            public string Title { get; init; }
            public bool Roles { get; init; }
            public int PendingLeads { get; init; }
            public int PendingFollows { get; init; }
            public int Pending { get; init; }
        }
    }
}
