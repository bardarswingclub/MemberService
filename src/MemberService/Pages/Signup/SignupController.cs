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
            var openEvents = await _database.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Where(e => e.Archived == false)
                .Where(e => e.SignupOptions.IsOpen())
                .ToListAsync();

            var futureEvents = await _database.Events
                .Include(e => e.SignupOptions)
                .AsNoTracking()
                .Expressionify()
                .Where(e => e.Archived == false)
                .Where(e => !e.SignupOptions.HasOpened() && !e.SignupOptions.HasClosed())
                .OrderBy(e => e.SignupOptions.SignupOpensAt)
                .ToListAsync();

            return View(new EventsModel
            {
                OpenEvents = openEvents,
                FutureEvents = futureEvents
            });
        }

        [HttpGet]
        [Route("Signup/Event/{id}/{slug?}")]
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

                var mustPayClassesFee = model.Options.RequiresClassesFee && !model.User.HasPayedClassesFeeThisSemester();
                var mustPayTrainingFee = model.Options.RequiresTrainingFee && !model.User.HasPayedTrainingFeeThisSemester();
                var mustPayMembershipFee = model.Options.RequiresMembershipFee && !model.User.HasPayedMembershipThisYear();

                var mustPayMembersPrice = model.Options.PriceForMembers > 0 && model.User.HasPayedMembershipThisYear();
                var mustPayNonMembersPrice = model.Options.PriceForNonMembers > 0 && !model.User.HasPayedMembershipThisYear();

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

            var user = await _database.Users
                .Include(u => u.EventSignups)
                .SingleUser(_userManager.GetUserId(User));

            user.EventSignups.Add(new EventSignup
            {
                EventId = id,
                Priority = 1,
                Role = input.Role,
                PartnerEmail = input.PartnerEmail,
                Status = Status.Pending,
                SignedUpAt = DateTime.UtcNow
            });

            await _database.SaveChangesAsync();

            return RedirectToAction(nameof(ThankYou), new { id });
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
                .SingleUser(_userManager.GetUserId(User));

            var signup = user.EventSignups.FirstOrDefault(s => s.EventId == id);

            if (signup?.Status == Status.Approved)
            {
                signup.Status = accept ? Status.AcceptedAndPayed : Status.RejectedOrNotPayed;
                await _database.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Event), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Success(Guid id)
        {
            if (TempData["StripeSessionId"] is string sessionId)
            {
                var user = await _database.Users
                .Include(u => u.EventSignups)
                .SingleUser(_userManager.GetUserId(User));

                var signup = user.EventSignups.FirstOrDefault(s => s.EventId == id);

                if (signup?.Status == Status.Approved && await _paymentService.SavePayment(sessionId) > 0)
                {
                    signup.Status = Status.AcceptedAndPayed;
                    await _database.SaveChangesAsync();
                }
            }

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
                Url.Action(nameof(Success), "Signup", new { id = model.Id }, Request.Scheme, Request.Host.Value),
                Request.GetDisplayUrl(),
                eventSignupId: model.UserEventSignup.Id);

            TempData["StripeSessionId"] = sessionId;

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
                Url.Action(nameof(Success), "Signup", new { id = model.Id }, Request.Scheme, Request.Host.Value),
                Request.GetDisplayUrl(),
                fee.IncludesMembership,
                fee.IncludesTraining,
                fee.IncludesClasses);

            TempData["StripeSessionId"] = sessionId;

            return sessionId;
        }
    }
}
