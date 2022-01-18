namespace MemberService.Components.EventHeader;

using Clave.Expressionify;

using MemberService.Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;

public class EventHeaderViewComponent : ViewComponent
{
    private readonly MemberContext _database;

    public EventHeaderViewComponent(MemberContext database)
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
            .Expressionify()
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
                HasClosed = e.HasClosed(),
                Archived = e.Archived,
                Cancelled = e.Cancelled
            })
            .FirstOrDefaultAsync(e => e.Id == id);

        return View(model);
    }

    public class Model
    {
        public Guid Id { get; set; }

        public String Title { get; set; }

        public string Description { get; set; }

        public bool Archived { get; set; }

        public bool Cancelled { get; set; }

        public bool HasClosed { get; set; }

        public bool IsOpen { get; set; }

        public bool IsSemester { get; set; }

        public Guid? SurveyId { get; set; }

        public DateTime? SignupOpensAt { get; set; }

        public DateTime? SignupClosesAt { get; set; }
    }
}
