namespace MemberService.Components.SemesterHistory;

using System;
using System.Threading.Tasks;

using Clave.ExtensionMethods;

using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;

public class SemesterHistory : ViewComponent
{
    private readonly MemberContext _database;

    public SemesterHistory(MemberContext database)
    {
        _database = database;
    }

    public async Task<IViewComponentResult> InvokeAsync(string userId, bool viewEvent = false)
    {
        var payments = await _database.Payments
            .Where(p => p.IncludesClasses || p.IncludesMembership || p.IncludesTraining)
            .Where(p => !p.Refunded)
            .Where(p => p.User.Id == userId)
            .ToListAsync();

        var semesters = await _database.Semesters
            .Include(s => s.Courses.Where(c => c.Signups.Any(s => s.UserId == userId)))
                .ThenInclude(c => c.Signups.Where(s => s.UserId == userId))
            .OrderByDescending(s => s.SignupOpensAt)
            .ToListAsync();

        return View(new Model
        {
            Semesters = semesters
                .Select(s => Model.Semester.Create(s, s.Courses.SelectMany(c => c.Signups), payments))
                .SkipWhile(s => s.Courses.NotAny() && !s.PaidMembership)
                .ToArray(),
            LinkToEvent = viewEvent
        });
    }

    public class Model
    {
        public bool LinkToEvent { get; init; }

        public IReadOnlyList<Semester> Semesters { get; init; }

        public class Semester
        {
            public string Title { get; init; }

            public IReadOnlyList<Course> Courses { get; init; }

            public bool PaidMembership { get; init; }

            public static Semester Create(
                Data.Semester semester,
                IEnumerable<EventSignup> events,
                IEnumerable<Payment> payments) => new()
                {
                    Title = semester.Title,
                    Courses = events
                    .OrderBy(c => c.Priority)
                    .Select(Course.Create)
                    .ToArray(),
                    PaidMembership = payments.Any(p => p.IncludesMembership && p.PayedAtUtc > semester.SignupOpensAt.GetStartOfYear() && p.PayedAtUtc < semester.SignupOpensAt.GetStartOfNextSemester())
                };

            public class Course
            {
                public Guid EventId { get; init; }
                public string Title { get; init; }
                public DanceRole Role { get; init; }
                public Status Status { get; init; }

                public static Course Create(EventSignup e) => new()
                {
                    EventId = e.EventId,
                    Title = e.Event.Title,
                    Role = e.Role,
                    Status = e.Status
                };
            }
        }
    }
}
