namespace MemberService.Pages.Signup;

using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Home;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class SignupController : Controller
{
    private readonly MemberContext _database;
    private readonly IStripePaymentService _stripePaymentService;

    public SignupController(
        MemberContext database,
        IStripePaymentService stripePaymentService)
    {
        _database = database;
        _stripePaymentService = stripePaymentService;
    }

    [AllowAnonymous]
    public IActionResult Index()
    {
        return RedirectToPage("/Home/Index");
    }

    [HttpGet]
    [Route("Signup/Event/{id}/{slug?}")]
    [AllowAnonymous]
    public async Task<IActionResult> Event(Guid id, bool preview = false)
    {
        var model = await _database.GetSignupModel(id);

        if (model == null)
        {
            return NotFound();
        }

        if (!User.Identity.IsAuthenticated)
        {
            if (model.HasClosed)
            {
                return View("ClosedAlready", model);
            }

            return View("Anonymous", model);
        }

        model.User = await _database.GetUser(GetUserId());

        if (model.User.EventSignups.FirstOrDefault(e => e.EventId == id) is EventSignup eventSignup)
        {
            model.UserEventSignup = eventSignup;

            if (eventSignup.Status != Status.Approved || model.IsCancelled || model.IsArchived)
            {
                return View("Status", model);
            }

            var acceptModel = await CreateAcceptModel(model);

            return base.View("Accept", acceptModel);
        }

        if (model.HasClosed || model.IsCancelled || model.IsArchived)
        {
            return View("ClosedAlready", model);
        }

        if (model.IsOpen || preview)
        {
            return View("Signup", model);
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

        var signup = user.AddEventSignup(id, input.Role, input.PartnerEmail, autoAccept);

        try
        {
            if (model.Survey != null)
            {
                signup.Response = new Response
                {
                    Survey = model.Survey,
                    User = user,
                    Answers = model.Survey.Questions
                        .JoinWithAnswers(input.Answers)
                        .ToList()
                };
            }
        }
        catch (ModelErrorException error)
        {
            ModelState.AddModelError(error.Key, error.Message);
            return RedirectToAction(nameof(Event), new { id });
        }

        await _database.SaveChangesAsync();

        if (!autoAccept)
        {
            TempData.SetSuccessMessage($"Du er nå påmeldt {model.Title}");
        }

        return RedirectToAction(nameof(Event), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> AcceptOrReject(Guid id, [FromForm] bool accept)
    {
        var signupModel = await _database.GetSignupModel(id);

        if (signupModel.IsArchived) return NotFound();

        var user = await _database.GetEditableUser(GetUserId());

        var signup = user.EventSignups.FirstOrDefault(s => s.EventId == id);

        if (accept)
        {
            if (signup?.Status == Status.Approved)
            {
                if (user.MustPayClassesFee(signupModel.Options)) return Forbid();
                if (user.MustPayTrainingFee(signupModel.Options)) return Forbid();
                if (user.MustPayMembershipFee(signupModel.Options)) return Forbid();
                if (user.MustPayMembersPrice(signupModel.Options)) return Forbid();
                if (user.MustPayNonMembersPrice(signupModel.Options)) return Forbid();

                signup.Status = Status.AcceptedAndPayed;
                signup.AuditLog.Add("Accepted", user);
            }
        }
        else
        {
            signup.Status = Status.RejectedOrNotPayed;
            signup.AuditLog.Add("Rejected", user);
        }

        await _database.SaveChangesAsync();

        return RedirectToAction(nameof(Event), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Refund(Guid id)
    {
        var userId = GetUserId();
        var signup = await _database.EventSignups
            .Where(s => s.Event.Cancelled && !s.Event.Archived)
            .Where(s => !s.Payment.Refunded)
            .FirstOrDefaultAsync(s => s.EventId == id && s.UserId == userId);

        if (signup?.Status == Status.AcceptedAndPayed)
        {
            await _stripePaymentService.Refund(signup.PaymentId);

            TempData.SetSuccessMessage($"Du vil få pengene tilbake på konto i løpet av noen dager");
        }

        return RedirectToAction(nameof(Event), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Success(Guid id, string sessionId)
    {
        var user = await _database.GetEditableUser(GetUserId());

        var signup = user.EventSignups.FirstOrDefault(s => s.EventId == id);

        await _stripePaymentService.SavePayment(sessionId);

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
            acceptModel.SessionId = await CreatePayment(model, classesFee, model.UserEventSignup.Id);
        }
        else if (mustPayTrainingFee)
        {
            var trainingFee = model.User.GetTrainingFee().Fee;
            acceptModel.MustPayAmount = trainingFee.Amount;
            acceptModel.SessionId = await CreatePayment(model, trainingFee, model.UserEventSignup.Id);
        }
        else if (mustPayMembershipFee)
        {
            var membershipFee = model.User.GetMembershipFee().Fee;
            acceptModel.MustPayAmount = membershipFee.Amount;
            acceptModel.SessionId = await CreatePayment(model, membershipFee, model.UserEventSignup.Id);
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
        var sessionId = await _stripePaymentService.CreatePaymentRequest(
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

    private async Task<string> CreatePayment(SignupModel model, Fee fee, Guid eventSignupId)
    {
        var sessionId = await _stripePaymentService.CreatePaymentRequest(
            model.User.FullName,
            model.User.Email,
            fee.Description,
            fee.Description,
            fee.Amount,
            SignupSuccessLink(model.Id),
            Request.GetDisplayUrl(),
            fee.IncludesMembership,
            fee.IncludesTraining,
            fee.IncludesClasses,
            eventSignupId: eventSignupId);

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

    private string GetUserId() => User.GetId();
}
