namespace MemberService.Components.EventHeader;

using MemberService.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;

public class EventHeader : ViewComponent
{
    private readonly MemberContext _database;

    public EventHeader(MemberContext database)
    {
        _database = database;
    }

    public async Task<IViewComponentResult> InvokeAsync(Guid? id)
    {
        if (id is null)
        {
            return new ContentViewComponentResult(string.Empty);
        }

        var model = await _database.Events
            .Select(e => new Model
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                SignupOpensAt = e.SignupOptions.SignupOpensAt,
                SignupClosesAt = e.SignupOptions.SignupClosesAt,
                SurveyId = e.SurveyId,
                IsSemester = e.SemesterId.HasValue,
                IsOpen = e.IsOpen(),
                HasOpened = e.HasOpened(),
                HasClosed = e.HasClosed(),
                Archived = e.Archived,
                Cancelled = e.Cancelled,
                Published = e.Published
            })
            .FirstOrDefaultAsync(e => e.Id == id);

        return View(model);
    }

    public class Model
    {
        public Guid Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public bool Published { get; init; }
        public bool Archived { get; init; }
        public bool Cancelled { get; init; }
        public bool HasOpened { get; init; }
        public bool HasClosed { get; init; }
        public bool IsOpen { get; init; }
        public bool IsSemester { get; init; }
        public Guid? SurveyId { get; init; }
        public DateTime? SignupOpensAt { get; init; }
        public DateTime? SignupClosesAt { get; init; }
        public bool CanEdit => !(Archived || Cancelled);
    }
}
