namespace MemberService.Pages.Signup;

using Clave.Expressionify;

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

        if (model is null) return NotFound();

        if (!User.Identity.IsAuthenticated)
        {
            if (model.HasClosed)
            {
                return View("ClosedAlready", new ClosedAlreadyModel
                {
                    Title = model.Title,
                    Description = model.Description,
                });
            }

            return View("Anonymous", new AnonymousModel
            {
                Id = id,
                Title = model.Title,
                Description = model.Description,
            });
        }

        model.User = await _database.GetUser(GetUserId());

        if (model.User.EventSignups.FirstOrDefault(e => e.EventId == id) is EventSignup eventSignup)
        {
            if (eventSignup.Status != Status.Approved || model.IsCancelled || model.IsArchived)
            {
                return View("Status", new StatusModel
                {
                    Id = id,
                    Title = model.Title,
                    Description = model.Description,
                    IsArchived = model.IsArchived,
                    IsCancelled = model.IsCancelled,
                    SurveyId = model.SurveyId,
                    Status = eventSignup.Status,
                    Refunded = eventSignup.Payment?.Refunded,
                    AllowPartnerSignup = model.Options.AllowPartnerSignup,
                    RoleSignup = model.Options.RoleSignup,
                    Role = eventSignup.Role,
                    PartnerEmail = eventSignup.PartnerEmail
                });
            }

            return View("Accept", CreateAcceptModel(new()
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description
            }, model.User, model.Options));
        }

        if (model.HasClosed || model.IsCancelled || model.IsArchived)
        {
            return View("ClosedAlready", new ClosedAlreadyModel
            {
                Title = model.Title,
                Description = model.Description,
            });
        }

        if (model.IsOpen || preview)
        {
            return View("Signup", model);
        }

        return View("NotOpenYet", new NotOpenYetModel
        {
            Title = model.Title,
            Description = model.Description,
            SignupHelp = model.Options.SignupHelp,
            SignupOpensAt = model.Options.SignupOpensAt
        });
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
        var model = await _database.Events
            .Include(e => e.SignupOptions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (model is null) return NotFound();

        if (model.Archived) return NotFound();

        var userId = GetUserId();

        var user = await _database.Users
            .Include(u => u.Payments)
            .SingleUser(userId);

        var signup = await _database.EventSignups
            .FirstOrDefaultAsync(s => s.EventId == id && s.UserId == userId);

        if (accept)
        {
            if (signup?.Status == Status.Approved)
            {
                var options = model.SignupOptions;

                if (user.MustPayClassesFee(options))
                {
                    return Redirect(await CreatePayment(
                        id,
                        user,
                        user.GetClassesFee().Fee));
                }
                else if (user.MustPayTrainingFee(options))
                {
                    return Redirect(await CreatePayment(
                        id,
                        user,
                        user.GetTrainingFee().Fee));
                }
                else if (user.MustPayMembershipFee(options))
                {
                    return Redirect(await CreatePayment(
                        id,
                        user,
                        user.GetMembershipFee().Fee));
                }
                else if (user.MustPayMembersPrice(options))
                {
                    return base.Redirect(await CreatePayment(
                        id,
                        model.Title,
                        model.Description,
                        user,
                        options.PriceForMembers));
                }
                else if (user.MustPayNonMembersPrice(options))
                {
                    return base.Redirect(await CreatePayment(
                        id,
                        model.Title,
                        model.Description,
                        user,
                        options.PriceForNonMembers));
                }
                else
                {
                    signup.Status = Status.AcceptedAndPayed;
                    signup.AuditLog.Add("Accepted", user);
                }
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
        await _stripePaymentService.SavePayment(sessionId);

        return RedirectToAction(nameof(Event), new { id });
    }

    private AcceptModel CreateAcceptModel(AcceptModel acceptModel, User user, EventSignupOptions options)
    {
        if (user.MustPayClassesFee(options))
        {
            return acceptModel with
            {
                Requirement = AcceptModel.AcceptanceRequirement.MustPayClassesFee,
                MustPayAmount = user.GetClassesFee().Fee.Amount
            };
        }
        else if (user.MustPayTrainingFee(options))
        {
            return acceptModel with
            {
                Requirement = AcceptModel.AcceptanceRequirement.MustPayTrainingFee,
                MustPayAmount = user.GetTrainingFee().Fee.Amount
            };
        }
        else if (user.MustPayMembershipFee(options))
        {
            return acceptModel with
            {
                Requirement = AcceptModel.AcceptanceRequirement.MustPayMembershipFee,
                MustPayAmount = user.GetMembershipFee().Fee.Amount
            };
        }
        else if (user.MustPayMembersPrice(options))
        {
            return acceptModel with
            {
                Requirement = AcceptModel.AcceptanceRequirement.MustPayMembersPrice,
                MustPayAmount = options.PriceForMembers
            };
        }
        else if (user.MustPayNonMembersPrice(options))
        {
            return acceptModel with
            {
                Requirement = AcceptModel.AcceptanceRequirement.MustPayNonMembersPrice,
                MustPayAmount = options.PriceForNonMembers
            };
        }

        return acceptModel;
    }

    private async Task<string> CreatePayment(Guid id, string title, string description, User user, decimal amount)
    {
        return await _stripePaymentService.CreatePaymentRequest(
            user.FullName,
            user.Email,
            title,
            description,
            amount,
            SignupSuccessLink(id),
            Request.GetDisplayUrl(),
            eventId: id);
    }

    private async Task<string> CreatePayment(Guid id, User user, Fee fee)
    {
        return await _stripePaymentService.CreatePaymentRequest(
            user.FullName,
            user.Email,
            fee.Description,
            fee.Description,
            fee.Amount,
            SignupSuccessLink(id),
            Request.GetDisplayUrl(),
            fee.IncludesMembership,
            fee.IncludesTraining,
            fee.IncludesClasses,
            eventId: id);
    }

    private string SignupSuccessLink(Guid id)
        => Url.ActionLink(nameof(Success), values: new { id, sessionId = "{CHECKOUT_SESSION_ID}" });

    private string GetUserId() => User.GetId();
}
