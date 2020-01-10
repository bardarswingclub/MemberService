using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Clave.Expressionify;
using Clave.ExtensionMethods;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Signup;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Home
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MemberContext _database;
        private readonly UserManager<User> _userManager;

        public HomeController(
            MemberContext database,
            UserManager<User> userManager)
        {
            _database = database;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var signups = await _database.EventSignups
                .AsNoTracking()
                .Expressionify()
                .Where(s => s.UserId == userId)
                .Where(s => s.Event.Semester != null)
                .Where(s => s.Event.Semester.IsActive())
                .Where(s => !s.Event.Archived)
                .OrderBy(s => s.Priority)
                .Select(s => CourseSignupModel.Create(s))
                .ToListAsync();

            return View(new IndexModel
            {
                Signups = signups
            });
        }

        public async Task<IActionResult> Signup()
        {
            var userId = _userManager.GetUserId(User);
            
            var semester = await _database.Semesters
                .Expressionify()
                .Where(s => s.IsActive())
                .FirstOrDefaultAsync();

            var courses = await _database.GetCourses(userId, e => e.HasOpened());

            var availableCourses = courses
                .Where(c => c.IsOpen)
                .Where(c => c.Signup == null)
                .OrderBy(c => c.Title)
                .ToReadOnlyList();

            var futureClasses = await _database.GetCourses(userId, e => e.WillOpen());

            var willOpenAt = futureClasses
                .WhereNotNull(e => e.OpensAt)
                .OrderBy(e => e.OpensAt)
                .Select(e => e.OpensAt)
                .FirstOrDefault();

            var sortable = courses
                .Select(c => c.Signup)
                .WhereNotNull()
                .NotAny(c => c.Status != Status.Pending);

            return View(new SignupModel
            {
                Courses = courses
                    .OrderBy(c => c.Signup?.Priority)
                    .ToReadOnlyList(),
                OpenClasses = availableCourses,
                OpensAt = semester.SignupOpensAt,
                Sortable = sortable
            });
        }

        [HttpPost]
        public async Task<IActionResult> Signup(
            [FromForm] IReadOnlyList<Guid> classes,
            [FromForm] IReadOnlyList<DanceRole> roles,
            [FromForm] IReadOnlyList<string> partners)
        {
            var items = new List<ClassSignup>();
            for (int i = 0; i < classes.Count; i++)
            {
                items.Add(new ClassSignup(classes[i], roles[i], partners[i], i + 1));
            }

            var userId = _userManager.GetUserId(User);
            var user = await _database.GetEditableUser(userId);

            var openClasses = await _database.GetCourses(userId, e => e.HasOpened());

            var classesNotSignedUpFor = openClasses
                .Where(c => c.Signup == null)
                .Select(c => c.Id)
                .ToReadOnlyList();

            var addedSignups = items
                .Where(i => classesNotSignedUpFor.Contains(i.Id))
                .ToReadOnlyList();

            foreach (var signup in addedSignups)
            {
                user.AddEventSignup(signup.Id, signup.Role, signup.PartnerEmail, false, signup.Priority);
            }

            var changedSignups = openClasses
                .WhereNotNull(c => c.Signup)
                .Join(items, c => c.Id, c => c.Id)
                .ToReadOnlyList();

            foreach(var (_, signup) in changedSignups)
            {
                var eventSignup = user.EventSignups.First(s => s.EventId == signup.Id);
                eventSignup.Priority = signup.Priority;
            }

            var removedSignups = openClasses
                .WhereNotNull(c => c.Signup)
                .Where(c => items.NotAny(i => i.Id == c.Id))
                .ToReadOnlyList();

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Signup));
        }

        [HttpGet]
        public async Task<IActionResult> Survey()
        {
            var userId = _userManager.GetUserId(User);

            var model = await _database.Semesters
                .Expressionify()
                .Where(s => s.IsActive())
                .Where(s => s.Survey != null)
                .Select(s => SurveyModel.Create(s, userId))
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Survey([FromForm] SurveyInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Survey));
            }
            
            var userId = _userManager.GetUserId(User);

            var model = await _database.Semesters
                .Include(s => s.Survey)
                .ThenInclude(s => s.Responses)
                .ThenInclude(r => r.Answers)
                .Expressionify()
                .Where(s => s.IsActive())
                .Where(s => s.Survey != null)
                .Select(s => new
                {
                    Survey = s.Survey,
                    Questions = s.Survey.Questions,
                    Responses = s.Survey.Responses.Where(r => r.UserId == userId)
                })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var response = model.Responses.FirstOrDefault(r => r.UserId == userId);

            if (response == null)
            {
                response = new Response
                {
                    UserId = userId
                };
                model.Survey.Responses.Add(response);
            }

            try
            {
                response.Answers = model.Survey.Questions
                    .JoinWithAnswers(input.Answers)
                    .ToList();
            }
            catch (ModelErrorException error)
            {
                ModelState.AddModelError(error.Key, error.Message);
                return RedirectToAction(nameof(Survey));
            }
            
            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Fees()
        {
            var user = await GetCurrentUser();

            return View(new FeesViewModel
            {
                MembershipFee = user.GetMembershipFee(),
                TrainingFee = user.GetTrainingFee(),
                ClassesFee = user.GetClassesFee()
            });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StatusCode(string statusCode)
        {
            return View();
        }

        private async Task<User> GetCurrentUser()
            => await _database.Users
                .Include(x => x.Payments)
                .SingleUser(_userManager.GetUserId(User));

        private class ClassSignup
        {
            public ClassSignup(Guid id, DanceRole role, string partnerEmail, int priority)
            {
                Id = id;
                Role = role;
                PartnerEmail = partnerEmail;
                Priority = priority;
            }

            public Guid Id { get; }

            public DanceRole Role { get; }

            public string PartnerEmail { get; }

            public int Priority { get; }
        }
    }
}
