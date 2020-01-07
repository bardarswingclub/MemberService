using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MemberService.Auth;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var semesters = await _database.Semesters
                .Where(Filter(all))
                .ToListAsync();

            return View(semesters);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateSemesterModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm]CreateSemesterModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var activeSemesters = await _database.Semesters.AnyAsync(Filter());

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