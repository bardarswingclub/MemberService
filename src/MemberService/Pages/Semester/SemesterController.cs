using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Clave.Expressionify;
using MemberService.Auth;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime.Text;

namespace MemberService.Pages.Semester
{
    [Authorize]
    public class SemesterController : Controller
    {
        private readonly MemberContext _database;

        public SemesterController(
            MemberContext database)
        {
            _database = database;
        }

        [HttpGet]
        [Permission.To(Permission.Action.View, Permission.Resource.Semester)]
        public async Task<IActionResult> Index(bool archived=false)
        {
            var semester = await _database.Semesters
                .Expressionify()
                .Where(s => s.IsActive())
                .OrderByDescending(s => s.SignupOpensAt)
                .Select(s => SemesterModel.Create(s, Filter(archived)))
                .FirstOrDefaultAsync();

            if (semester == null)
            {
                return View("Nothing");
            }

            return View(semester);
        }

        [HttpGet]
        [Permission.To(Permission.Action.Create, Permission.Resource.Semester)]
        public IActionResult Create()
        {
            var now = TimeProvider.UtcToday;
            var season = now.Month >= 7 ? "Høsten" : "Våren";
            var year = now.Year;

            var (date, _) = now.AddDays(7).GetLocalDateAndTime();

            var model = new SemesterInputModel
            {
                Title = $"{season} {year}",
                SignupOpensAtDate = date,
                SignupOpensAtTime = "12:00"
            };

            return View(model);
        }

        [HttpPost]
        [Permission.To(Permission.Action.Create, Permission.Resource.Semester)]
        public async Task<IActionResult> Create([FromForm]SemesterInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var activeSemesters = await _database.Semesters
                .Expressionify()
                .AnyAsync(s => s.IsActive());

            if (activeSemesters)
            {
                return RedirectToAction(nameof(Index));
            }

            _database.Semesters.Add(new Data.Semester
            {
                Title = input.Title,
                SignupOpensAt = GetUtc(input.SignupOpensAtDate, input.SignupOpensAtTime)
            });

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Permission.To(Permission.Action.Edit, Permission.Resource.Semester)]
        public async Task<IActionResult> Edit()
        {
            var semester = await _database.Semesters
                .Expressionify()
                .Where(s => s.IsActive())
                .OrderByDescending(s => s.SignupOpensAt)
                .FirstOrDefaultAsync();

            var (date, time) = semester.SignupOpensAt.GetLocalDateAndTime();

            var model = new SemesterInputModel
            {
                Title = semester.Title,
                SignupOpensAtDate = date,
                SignupOpensAtTime = time
            };

            return View(model);
        }

        [HttpPost]
        [Permission.To(Permission.Action.Edit, Permission.Resource.Semester)]
        public async Task<IActionResult> Edit([FromForm] SemesterInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var semester = await _database.Semesters
                .Include(s => s.Courses)
                .ThenInclude(c => c.SignupOptions)
                .Expressionify()
                .Where(s => s.IsActive())
                .OrderByDescending(s => s.SignupOpensAt)
                .FirstOrDefaultAsync();

            semester.Title = input.Title;
            semester.SignupOpensAt = GetUtc(input.SignupOpensAtDate, input.SignupOpensAtTime);

            foreach (var course in semester.Courses)
            {
                if (course.SignupOptions.SignupOpensAt < semester.SignupOpensAt)
                {
                    course.SignupOptions.SignupOpensAt = semester.SignupOpensAt;
                }
            }

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private static Expression<Func<Data.Event, bool>> Filter(bool all = false)
        {
            if (all)
            {
                return e => true;
            }

            return e => e.Archived == false;
        }

        internal static DateTime GetUtc(string date, string time)
        {
            var dateTime = $"{date}T{time}:00";

            var localDateTime = LocalDateTimePattern.GeneralIso.Parse(dateTime).GetValueOrThrow();

            return localDateTime.InZoneLeniently(TimeProvider.TimeZoneOslo).ToDateTimeUtc();
        }
    }
}