﻿namespace MemberService.Pages.Semester;

using System.Linq.Expressions;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Pages.Event;
using MemberService.Pages.Shared;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class SemesterController : Controller
{
    private readonly MemberContext _database;

    public SemesterController(MemberContext database)
    {
        _database = database;
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanViewSemester))]
    public async Task<IActionResult> Index(bool archived = false)
    {
        var semester = await _database.Semesters
            .Current(s => SemesterModel.Create(s, GetUserId(), Filter(archived)));

        if (semester == null)
        {
            return View("Nothing");
        }

        return View(semester);
    }

    [HttpGet("{controller}/{action}/{id}")]
    [Authorize(nameof(Policy.CanViewSemester))]
    public async Task<IActionResult> Index(Guid id, bool archived = false)
    {
        var semester = await _database.Semesters
            .Select(s => SemesterModel.Create(s, GetUserId(), Filter(archived)))
            .FirstOrDefaultAsync(s => s.Id == id);

        if (semester == null)
        {
            return NotFound();
        }

        return View(semester);
    }

    [Authorize(nameof(Policy.CanViewSemester))]
    public async Task<IActionResult> Export(Guid id)
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


        return CsvResult.Create(rows, "signups.csv");
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanViewSemester))]
    public async Task<IActionResult> List()
    {
        var semester = await _database.Semesters
            .OrderByDescending(s => s.SignupOpensAt)
            .Select(s => SemesterModel.Create(s, GetUserId(), e => true))
            .ToListAsync();

        return View(semester);
    }

    [HttpGet]
    [Authorize(nameof(Policy.CanCreateSemester))]
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
    [Authorize(nameof(Policy.CanCreateSemester))]
    public async Task<IActionResult> Create([FromForm] SemesterInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var activeSemesters = await _database.Semesters
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
    [Authorize(nameof(Policy.CanEditSemester))]
    public async Task<IActionResult> Edit()
    {
        var semester = await _database.Semesters.Current();

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
    [Authorize(nameof(Policy.CanEditSemester))]
    public async Task<IActionResult> Edit([FromForm] SemesterInputModel input)
    {
        if (!ModelState.IsValid)
        {
            return View(input);
        }

        var semester = await _database.Semesters
            .Include(s => s.Courses)
            .ThenInclude(c => c.SignupOptions)
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

    private string GetUserId() => User.GetId();

    private static Expression<Func<Data.Event, bool>> Filter(bool all = false)
    {
        if (all)
        {
            return e => true;
        }

        return e => e.Archived == false;
    }
}
