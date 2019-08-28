using System;
using System.Linq;
using System.Threading.Tasks;
using MemberService.Data;
using MemberService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MemberService.Pages.Signup
{
    [Authorize]
    public class SignupController : Controller
    {
        private readonly MemberContext _database;
        private readonly UserManager<MemberUser> _userManager;
        private readonly IPaymentService _paymentService;

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
            var userId = GetUserId();

            var openEvents = await _database.GetEvents(userId, e => e.IsOpen() && e.Type != EventType.Class);

            var futureEvents = await _database.GetEvents(userId, e => e.WillOpen() && e.Type != EventType.Class);

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
            var model = await _database.GetSignupModel(id);

            if (model == null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return View("Anonymous", model);
            }

            model.User = await _database.GetUser(GetUserId());

            if (model.User.EventSignups.FirstOrDefault(e => e.EventId == id) is EventSignup eventSignup)
            {
                model.UserEventSignup = eventSignup;

                if (eventSignup.Status != Status.Approved)
                {
                    return View("Status", model);
                }

                var acceptModel = await CreateAcceptModel(model);

                return base.View("Accept", acceptModel);
            }

            if (model.IsOpen)
            {
                return View("Signup", model);
            }

            if (model.HasClosed)
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

            var model = await _database.GetEditableEvent(id);

            if (model == null)
            {
                return NotFound();
            }

            if (!model.IsOpen())
            {
                return RedirectToAction(nameof(Event), new { id });
            }

            var user = await _database.GetEditableUser(GetUserId());

            if (user.IsSignedUpFor(id))
            {
                return RedirectToAction(nameof(Event), new { id });
            }

            var autoAccept = model.ShouldAutoAccept(input.Role);

            user.AddEventSignup(id, input, autoAccept);

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
            var model = await _database.GetSignupModel(id);

            if (model == null)
            {
                return NotFound();
            }

            model.User = await _database.GetUser(GetUserId());

            if (model.User.GetEditableEvent(id) is EventSignup eventSignup)
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

            if (await _database.GetEditableEvent(id) == null)
            {
                return NotFound();
            }

            var user = await _database.GetEditableUser(GetUserId());

            if (user.GetEditableEvent(id) is EventSignup eventSignup)
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
            var model = await _database.GetSignupModel(id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptOrReject(Guid id, [FromForm] bool accept)
        {
            var user = await _database.GetEditableUser(GetUserId());

            var signupModel = await _database.GetSignupModel(id);

            if (user.MustPayClassesFee(signupModel.Options)) return Forbid();
            if (user.MustPayTrainingFee(signupModel.Options)) return Forbid();
            if (user.MustPayMembershipFee(signupModel.Options)) return Forbid();
            if (user.MustPayMembersPrice(signupModel.Options)) return Forbid();
            if (user.MustPayNonMembersPrice(signupModel.Options)) return Forbid();

            var signup = user.EventSignups.FirstOrDefault(s => s.EventId == id);

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
            var user = await _database.GetEditableUser(GetUserId());

            var signup = user.EventSignups.FirstOrDefault(s => s.EventId == id);

            await _paymentService.SavePayment(sessionId);

            return RedirectToAction(nameof(Event), new { id });
        }

        private async Task<AcceptModel> CreateAcceptModel(SignupModel model)
        {
            var mustPayClassesFee = model.User.MustPayClassesFee(model.Options);
            var mustPayTrainingFee = model.User.MustPayTrainingFee(model.Options);
            var mustPayMembershipFee = model.User.MustPayMembershipFee(model.Options);

            var mustPayMembersPrice = model.User.MustPayMembersPrice(model.Options);
            var mustPayNonMembersPrice = model.User.MustPayNonMembersPrice(model.Options);

            var acceptModel = new AcceptModel
            {
                Id = model.Id,
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

            return acceptModel;
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

        private string GetUserId() => _userManager.GetUserId(User);
    }
}
