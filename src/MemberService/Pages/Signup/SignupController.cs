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
        var ev = await _database.Events
            .Expressionify()
            .Where(e => e.Id == id)
            .Select(e => new
            {
                Title = e.Title,
                Description = e.Description,
                HasClosed = e.HasClosed(),
                IsCancelled = e.Cancelled,
                IsArchived = e.Archived || !e.Semester.IsActive(),
                IsOpen = e.IsOpen(),
                Options = e.SignupOptions,
                SurveyId = e.SurveyId
            })
            .FirstOrDefaultAsync();

        if (ev is null) return NotFound();

        var user = User.Identity.IsAuthenticated ? await _database.GetUser(GetUserId()) : null;

        if (user?.EventSignups.FirstOrDefault(e => e.EventId == id) is EventSignup eventSignup)
        {
            if (eventSignup.Status != Status.Approved || ev.IsCancelled || ev.IsArchived)
            {
                return View("Status", new StatusModel
                {
                    Id = id,
                    Title = ev.Title,
                    Description = ev.Description,
                    IsArchived = ev.IsArchived,
                    IsCancelled = ev.IsCancelled,
                    SurveyId = ev.SurveyId,
                    Status = eventSignup.Status,
                    Refunded = eventSignup.Payment?.Refunded,
                    AllowPartnerSignup = ev.Options.AllowPartnerSignup,
                    RoleSignup = ev.Options.RoleSignup,
                    Role = eventSignup.Role,
                    PartnerEmail = eventSignup.PartnerEmail,
                    CanEdit = eventSignup.CanEdit() && !ev.IsArchived && (ev.Options.RoleSignup || ev.SurveyId.HasValue)
                });
            }

            return View("Accept", CreateAcceptModel(new()
            {
                Id = id,
                Title = ev.Title,
                Description = ev.Description
            }, user, ev.Options));
        }

        if (ev.HasClosed || ev.IsCancelled || ev.IsArchived)
        {
            return View("ClosedAlready", new ClosedAlreadyModel
            {
                Title = ev.Title,
                Description = ev.Description,
            });
        }

        if (ev.IsOpen || preview)
        {
            if (user is null)
            {
                return View("Anonymous", new AnonymousModel
                {
                    Id = id,
                    Title = ev.Title,
                    Description = ev.Description,
                });
            }
            else
            {
                return View("Signup", new SignupModel
                {
                    Title = ev.Title,
                    Description = ev.Description,
                    AllowPartnerSignup = ev.Options.AllowPartnerSignup,
                    Requirement = GetRequirement(user, ev.Options),
                    AllowPartnerSignupHelp = ev.Options.AllowPartnerSignupHelp,
                    PriceForMembers = ev.Options.PriceForMembers,
                    PriceForNonMembers = ev.Options.PriceForNonMembers,
                    RoleSignup = ev.Options.RoleSignup,
                    RoleSignupHelp = ev.Options.RoleSignupHelp,
                    SignupHelp = ev.Options.SignupHelp,
                    SurveyId = ev.SurveyId
                });
            }
        }

        return View("NotOpenYet", new NotOpenYetModel
        {
            Title = ev.Title,
            Description = ev.Description,
            SignupHelp = ev.Options.SignupHelp,
            SignupOpensAt = ev.Options.SignupOpensAt
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

                var requirement = GetRequirement(user, options);

                if(requirement == SignupRequirement.None)
                {
                    signup.Status = Status.AcceptedAndPayed;
                    signup.AuditLog.Add("Accepted", user);
                }
                else
                {
                    return requirement switch
                    {
                        SignupRequirement.MustPayClassesFee => Redirect(await CreatePayment(
                            id,
                            user,
                            user.GetClassesFee().Fee)),
                        SignupRequirement.MustPayTrainingFee => Redirect(await CreatePayment(
                            id,
                            user,
                            user.GetTrainingFee().Fee)),
                        SignupRequirement.MustPayMembershipFee => Redirect(await CreatePayment(
                            id,
                            user,
                            user.GetMembershipFee().Fee)),
                        SignupRequirement.MustPayMembersPrice => Redirect(await CreatePayment(
                            id,
                            model.Title,
                            model.Description,
                            user,
                            options.PriceForMembers)),
                        SignupRequirement.MustPayNonMembersPrice => Redirect(await CreatePayment(
                            id,
                            model.Title,
                            model.Description,
                            user,
                            options.PriceForNonMembers)),
                        _ => throw new Exception(),
                    };
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
        return GetRequirement(user, options) switch
        {
            SignupRequirement.MustPayClassesFee => acceptModel with
            {
                Requirement = SignupRequirement.MustPayClassesFee,
                MustPayAmount = user.GetClassesFee().Fee.Amount
            },
            SignupRequirement.MustPayTrainingFee => acceptModel with
            {
                Requirement = SignupRequirement.MustPayTrainingFee,
                MustPayAmount = user.GetTrainingFee().Fee.Amount
            },
            SignupRequirement.MustPayMembershipFee => acceptModel with
            {
                Requirement = SignupRequirement.MustPayMembershipFee,
                MustPayAmount = user.GetMembershipFee().Fee.Amount
            },
            SignupRequirement.MustPayMembersPrice => acceptModel with
            {
                Requirement = SignupRequirement.MustPayMembersPrice,
                MustPayAmount = options.PriceForMembers
            },
            SignupRequirement.MustPayNonMembersPrice => acceptModel with
            {
                Requirement = SignupRequirement.MustPayNonMembersPrice,
                MustPayAmount = options.PriceForNonMembers
            },
            _ => acceptModel,
        };
    }

    private static SignupRequirement GetRequirement(User user, EventSignupOptions options)
    {
        if (user.MustPayClassesFee(options))
        {
            return SignupRequirement.MustPayClassesFee;
        }
        else if (user.MustPayTrainingFee(options))
        {
            return SignupRequirement.MustPayTrainingFee;
        }
        else if (user.MustPayMembershipFee(options))
        {
            return SignupRequirement.MustPayMembershipFee;
        }
        else if (user.MustPayMembersPrice(options))
        {
            return SignupRequirement.MustPayMembersPrice;
        }
        else if (user.MustPayNonMembersPrice(options))
        {
            return SignupRequirement.MustPayNonMembersPrice;
        }

        return SignupRequirement.None;
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
