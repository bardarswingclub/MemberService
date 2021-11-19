using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Clave.Expressionify;
using MemberService.Auth;
using MemberService.Data;
using MemberService.Pages.Event;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Index(bool archived = false)
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

        [HttpGet("{controller}/{action}/{id}")]
        public async Task<IActionResult> Index(Guid id, bool archived = false)
        {
            var semester = await _database.Semesters
                .Expressionify()
                .Select(s => SemesterModel.Create(s, Filter(archived)))
                .FirstOrDefaultAsync(s => s.Id == id);

            if (semester == null)
            {
                return NotFound();
            }

            return View(semester);
        }

        public async Task<object> Export(Guid id)
        {
            var rows = await _database.Events
                .Where(e => e.SemesterId == id)
                .SelectMany(
                e => e.Signups.Select(
                    s => new
                    {
                        Course = e.Title,
                        s.User.Email,
                        s.User.FullName,
                        s.Priority,
                        s.Role,
                        s.PartnerEmail,
                        s.SignedUpAt,
                        Status = s.Status.ToString()
                    }))
                .ToListAsync();


            return new FileContentResult(Encoding.UTF8.GetBytes(rows.ToCsv()), "text/csv")
            {
                FileDownloadName = "signups.csv"
            };
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var semester = await _database.Semesters
                .Expressionify()
                .OrderByDescending(s => s.SignupOpensAt)
                .Select(s => SemesterModel.Create(s, e => true))
                .ToListAsync();

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

            var model = new SemesterInputModel
            {
                Title = $"{season} {year}",
                SignupOpensAtDate = date,
                SignupOpensAtTime = "12:00"
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(nameof(Policy.IsCoordinator))]
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
                SignupOpensAt = input.SignupOpensAtDate.GetUtc(input.SignupOpensAtTime),
                SignupHelpText = input.SignupHelpText
            });

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
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
                SignupOpensAtTime = time,
                SignupHelpText = semester.SignupHelpText
            };

            return View(model);
        }

        [HttpPost]
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
            semester.SignupOpensAt = input.SignupOpensAtDate.GetUtc(input.SignupOpensAtTime);
            semester.SignupHelpText = input.SignupHelpText;

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
    }
}
