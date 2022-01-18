namespace MemberService.Components.SemesterHeader;

using MemberService.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;

public class SemesterHeaderViewComponent : ViewComponent
{
    private readonly MemberContext _database;

    public SemesterHeaderViewComponent(MemberContext database)
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
                        Title = c.Title
                    })
                    .OrderBy(c => c.Title)
                    .ToList()
            })
            .FirstOrDefaultAsync(e => e.Id == id.Value);

        return View(model);
    }

    public class Model
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public bool IsActive { get; set; }

        public Guid? SurveyId { get; set; }

        public IReadOnlyCollection<Event> Events { get; set; }

        public class Event
        {
            public Guid Id { get; set; }

            public string Title { get; set; }
        }
    }
}
