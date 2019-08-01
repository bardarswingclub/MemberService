using System;
using System.Linq;
using System.Threading.Tasks;
using Clave.Expressionify;
using MemberService.Data;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemberService.Pages.Signup
{
    [Authorize]
    public class SignupController : Controller
    {
        private MemberContext _database;
        private UserManager<MemberUser> _userManager;
        private IPaymentService _paymentService;

        public SignupController(
            MemberContext database,
            UserManager<MemberUser> userManager,
            IPaymentService paymentService)
        {
            _database = database;
            _userManager = userManager;
            _paymentService = paymentService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var openEvents = await _database.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Where(e => e.Archived == false)
                .Where(e => e.SignupOptions.IsOpen())
                .Select(e => EventModel.Create(e, userId))
                .ToListAsync();

            var futureEvents = await _database.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Where(e => e.Archived == false)
                .Where(e => !e.SignupOptions.HasOpened() && !e.SignupOptions.HasClosed())
                .OrderBy(e => e.SignupOptions.SignupOpensAt)
                .Select(e => EventModel.Create(e, userId))
                .ToListAsync();

            return View(new EventsModel
            {
                OpenEvents = openEvents,
                FutureEvents = futureEvents
            });
        }

        [HttpGet]
        [Route("Signup/Event/{id}/{slug?}")]
        [AllowAnonymous]
        public async Task<IActionResult> Event(Guid id)
        {
            var model = await _database.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Select(e => SignupModel.Create(e))
                .SingleOrDefaultAsync(e => e.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return View("Anonymous", model);
            }

            model.User = await _database.Users
                .Include(u => u.Payments)
                .Include(u => u.EventSignups)
                .AsNoTracking()
                .SingleUser(_userManager.GetUserId(User));

            if (model.User.EventSignups.FirstOrDefault(e => e.EventId == id) is EventSignup eventSignup)
            {
                model.UserEventSignup = eventSignup;

                if (eventSignup.Status != Status.Approved)
                {
                    return View("Status", model);
                }

                var mustPayClassesFee = MustPayClassesFee(model.Options, model.User);
                var mustPayTrainingFee = MustPayTrainingFee(model.Options, model.User);
                var mustPayMembershipFee = MustPayMembershipFee(model.Options, model.User);

                var mustPayMembersPrice = MustPayMembersPrice(model.Options, model.User);
                var mustPayNonMembersPrice = MustPayNonMembersPrice(model.Options, model.User);

                var acceptModel = new AcceptModel
                {
                    Id = id,
                    Title = model.Title,
                    Description = model.Description,
                    MustPayClassesFee = mustPayClassesFee,
                    MustPayTrainingFee = mustPayTrainingFee,
                    MustPayMembershipFee = mustPayMembershipFee
                };

                if (mustPayClassesFee)
                {
                    var classesFee = model.User.GetClassesFee().Fee;
                    acceptModel.MustPayAmount = classesFee.Amount;
                    acceptModel.SessionId = await CreatePayment(model, classesFee);
                }
                else if (mustPayTrainingFee)
                {
                    var trainingFee = model.User.GetTrainingFee().Fee;
                    acceptModel.MustPayAmount = trainingFee.Amount;
                    acceptModel.SessionId = await CreatePayment(model, trainingFee);
                }
                else if (mustPayMembershipFee)
                {
                    var membershipFee = model.User.GetMembershipFee().Fee;
                    acceptModel.MustPayAmount = membershipFee.Amount;
                    acceptModel.SessionId = await CreatePayment(model, membershipFee);
                }
                else if (mustPayMembersPrice)
                {
                    acceptModel.MustPayAmount = model.Options.PriceForMembers;
                    acceptModel.SessionId = await CreatePayment(model, model.Options.PriceForMembers);
                }
                else if (mustPayNonMembersPrice)
                {
                    acceptModel.MustPayAmount = model.Options.PriceForNonMembers;
                    acceptModel.SessionId = await CreatePayment(model, model.Options.PriceForNonMembers);
                }

                return base.View("Accept", acceptModel);
            }

            if (model.Options.IsOpen())
            {
                return View("Signup", model);
            }

            if (model.Options.HasClosed())
            {
                return View("ClosedAlready", model);
            }

            return View("NotOpenYet", model);
        }

        [HttpPost]
        [Route("Signup/Event/{id}/{slug?}")]
        public async Task<IActionResult> Event(Guid id, [FromForm] SignupInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Event), new { id });
            }

            var ev = await _database.Events
                .Include(e => e.Signups)
                .Include(e => e.SignupOptions)
                .SingleOrDefaultAsync(e => e.Id == id);

            if (ev == null)
            {
                return NotFound();
            }

            if (!ev.SignupOptions.IsOpen())
            {
                return RedirectToAction(nameof(Event), new { id });
            }

            var user = await _database.Users
                .Include(u => u.EventSignups)
                .SingleUser(_userManager.GetUserId(User));

            if (user.EventSignups.FirstOrDefault(e => e.EventId == id) != null)
            {
                return RedirectToAction(nameof(Event), new { id });
            }

            var autoAccept = ev.SignupOptions.AutoAcceptedSignups > ev.Signups.Count(s => s.Role == input.Role);
            var status = autoAccept ? Status.Approved : Status.Pending;

            user.EventSignups.Add(new EventSignup
            {
                EventId = id,
                Priority = 1,
                Role = input.Role,
                PartnerEmail = input.PartnerEmail?.Normalize().ToUpperInvariant(),
                Status = status,
                SignedUpAt = DateTime.UtcNow,
                AuditLog =
                {
                    { $"Signed up as {input.Role}{(input.PartnerEmail is string partnerEmail ? $" with partner {partnerEmail}" : "")}, status is {status}", user }
                }
            });

            await _database.SaveChangesAsync();

            if (autoAccept)
            {
                return RedirectToAction(nameof(Event), new { id });
            }
            else
            {
                return RedirectToAction(nameof(ThankYou), new { id });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var model = await _database.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Select(e => SignupModel.Create(e))
                .SingleOrDefaultAsync(e => e.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            model.User = await _database.Users
                .Include(u => u.Payments)
                .Include(u => u.EventSignups)
                .AsNoTracking()
                .SingleUser(_userManager.GetUserId(User));

            if (model.User.EventSignups.Where(CanEdit).FirstOrDefault(e => e.EventId == id) is EventSignup eventSignup)
            {
                model.UserEventSignup = eventSignup;
                model.Input = new SignupInputModel
                {
                    Role = eventSignup.Role,
                    PartnerEmail = eventSignup.PartnerEmail
                };

                return View("Edit", model);
            }

            return RedirectToAction(nameof(Event), new { id, slug = model.Title.Slugify() });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [FromForm] SignupInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit), new { id });
            }

            var ev = await _database.Events
                .Include(e => e.Signups)
                .Include(e => e.SignupOptions)
                .SingleOrDefaultAsync(e => e.Id == id);

            if (ev == null)
            {
                return NotFound();
            }

            var user = await _database.Users
                .Include(u => u.EventSignups)
                .SingleUser(_userManager.GetUserId(User));

            if (user.EventSignups.Where(CanEdit).FirstOrDefault(e => e.EventId == id) is EventSignup eventSignup)
            {
                eventSignup.AuditLog.Add($"Changed signup\n\n{eventSignup.Role} -> {input.Role}\n\n{eventSignup.PartnerEmail} -> {input.PartnerEmail}", user);

                eventSignup.Role = input.Role;
                eventSignup.PartnerEmail = input.PartnerEmail?.Normalize().ToUpperInvariant();

                await _database.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Event), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> ThankYou(Guid id)
        {
            var model = await _database.Events
                .Expressionify()
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Select(e => SignupModel.Create(e))
                .SingleOrDefaultAsync(e => e.Id == id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptOrReject(Guid id, [FromForm] bool accept)
        {
            var user = await _database.Users
                .Include(u => u.EventSignups)
                    .ThenInclude(s => s.Event)
                        .ThenInclude(e => e.SignupOptions)
                .SingleUser(_userManager.GetUserId(User));

            var signup = user.EventSignups.FirstOrDefault(s => s.EventId == id);

            if (MustPayClassesFee(signup.Event.SignupOptions, user)) return Forbid();
            if (MustPayTrainingFee(signup.Event.SignupOptions, user)) return Forbid();
            if (MustPayMembershipFee(signup.Event.SignupOptions, user)) return Forbid();
            if (MustPayMembersPrice(signup.Event.SignupOptions, user)) return Forbid();
            if (MustPayNonMembersPrice(signup.Event.SignupOptions, user)) return Forbid();

            if (signup?.Status == Status.Approved)
            {
                if (accept)
                {
                    signup.Status = Status.AcceptedAndPayed;
                    signup.AuditLog.Add("Accepted", user);
                }
                else
                {
                    signup.Status = Status.RejectedOrNotPayed;
                    signup.AuditLog.Add("Rejected", user);
                }

                await _database.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Event), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Success(Guid id, string sessionId)
        {
            var user = await _database.Users
            .Include(u => u.EventSignups)
            .SingleUser(_userManager.GetUserId(User));

            var signup = user.EventSignups.FirstOrDefault(s => s.EventId == id);

            await _paymentService.SavePayment(sessionId);

            return RedirectToAction(nameof(Event), new { id });
        }

        private async Task<string> CreatePayment(SignupModel model, decimal amount)
        {
            var sessionId = await _paymentService.CreatePayment(
                model.User.FullName,
                model.User.Email,
                model.Title,
                model.Description,
                amount,
                SignupSuccessLink(model.Id),
                Request.GetDisplayUrl(),
                eventSignupId: model.UserEventSignup.Id);

            return sessionId;
        }

        private async Task<string> CreatePayment(SignupModel model, Fee fee)
        {
            var sessionId = await _paymentService.CreatePayment(
                model.User.FullName,
                model.User.Email,
                fee.Description,
                fee.Description,
                fee.Amount,
                SignupSuccessLink(model.Id),
                Request.GetDisplayUrl(),
                fee.IncludesMembership,
                fee.IncludesTraining,
                fee.IncludesClasses);

            return sessionId;
        }

        private string SignupSuccessLink(Guid id)
            => Url.ActionLink(
                nameof(Success),
                "Signup",
                new
                {
                    id,
                    sessionId = "{CHECKOUT_SESSION_ID}"
                });

        private static bool CanEdit(EventSignup e) => e.Status == Status.Pending || e.Status == Status.Recommended;

        private static bool MustPayNonMembersPrice(EventSignupOptions options, MemberUser user) => options.PriceForNonMembers > 0 && !user.HasPayedMembershipThisYear();

        private static bool MustPayMembersPrice(EventSignupOptions options, MemberUser user) => options.PriceForMembers > 0 && user.HasPayedMembershipThisYear();

        private static bool MustPayMembershipFee(EventSignupOptions options, MemberUser user) => options.RequiresMembershipFee && !user.HasPayedMembershipThisYear();

        private static bool MustPayTrainingFee(EventSignupOptions options, MemberUser user) => options.RequiresTrainingFee && !user.HasPayedTrainingFeeThisSemester();

        private static bool MustPayClassesFee(EventSignupOptions options, MemberUser user) => options.RequiresClassesFee && !user.HasPayedClassesFeeThisSemester();
    }
}
