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
using NodaTime.Extensions;
using NodaTime.Text;

namespace MemberService.Pages.Semester
{
    [Authorize(nameof(Policy.IsInstructor))]
    public class SemesterController : Controller
    {
        private readonly MemberContext _database;

        public SemesterController(
            MemberContext database)
        {
            _database = database;
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool all = false)
        {
            var semester = await _database.Semesters
                .Include(s => s.Courses)
                .ThenInclude(c => c.Signups)
                .Include(s => s.Courses)
                .ThenInclude(c => c.SignupOptions)
                .Include(s => s.Survey)
                .Expressionify()
                .Where(Filter(all))
                .Select(s => SemesterModel.Create(s))
                .FirstOrDefaultAsync();

            if (semester == null)
            {
                return View("Nothing");
            }

            return View(semester);
        }

        [HttpGet]
        [Authorize(nameof(Policy.IsCoordinator))]
        public IActionResult Create()
        {
            var now = TimeProvider.UtcToday;
            var season = now.Month >= 7 ? "Høsten" : "Våren";
            var year = now.Year;

            var (date, _) = now.AddDays(7).GetLocalDateAndTime();

            var model = new CreateSemesterModel
            {
                Title = $"{season} {year}",
                SignupOpensAtDate = date,
                SignupOpensAtTime = "12:00"
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
        public async Task<IActionResult> Create([FromForm]CreateSemesterModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var activeSemesters = await _database.Semesters
                .Expressionify()
                .AnyAsync(Filter());

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

        private static Expression<Func<Data.Semester, bool>> Filter(bool all = false)
        {
            if (all)
            {
                return s => true;
            }

            return s => s.IsActive();
        }

        internal static DateTime GetUtc(string date, string time)
        {
            var dateTime = $"{date}T{time}:00";

            var localDateTime = LocalDateTimePattern.GeneralIso.Parse(dateTime).GetValueOrThrow();

            return localDateTime.InZoneLeniently(TimeProvider.TimeZoneOslo).ToDateTimeUtc();
        }
    }
}