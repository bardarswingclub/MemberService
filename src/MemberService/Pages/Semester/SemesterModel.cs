namespace MemberService.Pages.Semester;




using System.Linq.Expressions;

using Clave.Expressionify;

using MemberService.Pages.Event;

public partial class SemesterModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public IReadOnlyList<EventEntry> Courses { get; set; }

    [Expressionify]
    public static SemesterModel Create(Data.Semester s, string userId, Expression<Func<Data.Event, bool>> filter) =>
        new()
        {
            Id = s.Id,
            Title = s.Title,
            Courses = s.Courses
                .AsQueryable()
                .Where(filter)
                .OrderBy(e => e.Title)
                .Select(e => EventEntry.Create(e, userId))
                .ToList()
        };
}
