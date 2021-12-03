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

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var model = await _database.GetIndexModel(userId);

            var openEvents = await _database.GetEvents(userId, e => e.HasOpened());

            var futureEvents = await _database.GetEvents(userId, e => e.WillOpen());

            model.PartyModel = new EventsModel
            {
                Title = "Fester",
                OpenEvents = openEvents.Where(e => e.Type == EventType.Party).ToList(),
                FutureEvents = futureEvents.Where(e => e.Type == EventType.Party).ToList()
            };

            model.WorkshopModel = new EventsModel
            {
                Title = "Workshops",
                OpenEvents = openEvents.Where(e => e.Type == EventType.Workshop).ToList(),
                FutureEvents = futureEvents.Where(e => e.Type == EventType.Workshop).ToList()
            };

            model.TrainingModel = new EventsModel
            {
                Title = "Egentrening",
                OpenEvents = openEvents.Where(e => e.Type == EventType.Training).ToList(),
                FutureEvents = futureEvents.Where(e => e.Type == EventType.Training).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> Signup()
        {
            var semester = await _database.Semesters
                .Expressionify()
                .Where(s => s.IsActive())
                .Select(s => new SignupInputModel
                {
                    SignupOpensAt = s.SignupOpensAt,
                    SignupHelpText = s.SignupHelpText

                })
                .FirstOrDefaultAsync();

            if (semester == null)
            {
                return View("NoSemester");
            }

            if (semester.SignupOpensAt > TimeProvider.UtcNow)
            {
                return View("NotOpenYet", new NotOpenYetModel { SignupOpensAt = semester.SignupOpensAt });
            }

            return View(semester);
        }

        [HttpPost]
        public IActionResult Signup([FromForm] SignupInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Signup));
            }

            return RedirectToAction(nameof(Courses));
        }

        public async Task<IActionResult> Courses()
        {
            var userId = _userManager.GetUserId(User);

            var semester = await _database.Semesters
                .Expressionify()
                .Where(s => s.IsActive())
                .FirstOrDefaultAsync();

            if (semester == null)
            {
                return View("NoSemester");
            }

            var preview = Request.Query.ContainsKey("preview") && User.IsInAnyRole(Roles.COORDINATOR, Roles.ADMIN);

            if (semester.SignupOpensAt > TimeProvider.UtcNow && !preview)
            {
                return View("NotOpenYet", new NotOpenYetModel { SignupOpensAt = semester.SignupOpensAt });
            }

            var courses = await _database.GetCourses(userId, e => e.HasOpened() || preview);

            var availableCourses = courses
                .Where(c => c.IsOpen || preview)
                .Where(c => c.Signup == null)
                .OrderBy(c => c.Title)
                .ToReadOnlyList();

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
                Sortable = sortable,
                SignupHelpText = semester.SignupHelpText
            });
        }

        [HttpPost]
        public async Task<IActionResult> Courses(
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

            foreach (var (_, signup) in changedSignups)
            {
                var eventSignup = user.EventSignups.First(s => s.EventId == signup.Id);
                eventSignup.Priority = signup.Priority;
            }

            var removedSignups = openClasses
                .WhereNotNull(c => c.Signup)
                .Where(c => items.NotAny(i => i.Id == c.Id))
                .ToReadOnlyList();

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(Survey));
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
                    Responses = s.Survey.Responses.Where(r => r.UserId == userId).ToList()
                })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var response = model.Responses.GetOrAdd(r => r.UserId == userId, () => new Response { UserId = userId });

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
