namespace MemberService.Pages.Members;




using System.Linq.Expressions;


using Clave.Expressionify;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class MembersController : Controller
{
    private readonly MemberContext _memberContext;
    private readonly UserManager<User> _userManager;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IAuthorizationService _authorizationService;

    public MembersController(
        MemberContext memberContext,
        UserManager<User> userManager,
        IPaymentService paymentService,
        IEmailService emailService,
        IAuthorizationService authorizationService)
    {
        _memberContext = memberContext;
        _userManager = userManager;
        _paymentService = paymentService;
        _emailService = emailService;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> Index(
        string memberFilter,
        string trainingFilter,
        string classesFilter,
        string exemptTrainingFilter,
        string exemptClassesFilter,
        string query)
    {
        var users = await _memberContext.Users
            .Include(u => u.Payments)
            .AsNoTracking()
            .Expressionify()
            .Where(u => u.EmailConfirmed)
            .Where(Filter(memberFilter, u => u.HasPayedMembershipThisYear()))
            .Where(FilterLastYear(memberFilter, u => u.HasPayedMembershipLastYear()))
            .Where(Filter(trainingFilter, u => u.HasPayedTrainingFeeThisSemester()))
            .Where(Filter(classesFilter, u => u.HasPayedClassesFeeThisSemester()))
            .Where(Filter(exemptTrainingFilter, u => u.ExemptFromTrainingFee))
            .Where(Filter(exemptClassesFilter, u => u.ExemptFromClassesFee))
            .Where(Search(query))
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return View(new MembersViewModel
        {
            Users = users
                .GroupBy(u => u.FullName?.ToUpper().FirstOrDefault() ?? '?', (key, u) => (key, u.ToReadOnlyCollection()))
                .ToReadOnlyCollection(),
            MemberFilter = memberFilter,
            TrainingFilter = trainingFilter,
            ClassesFilter = classesFilter,
            ExemptTrainingFilter = exemptTrainingFilter,
            ExemptClassesFilter = exemptClassesFilter,
            Query = query
        });
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _memberContext.Users
            .Include(u => u.Payments)
            .Include(u => u.UserRoles)
                .ThenInclude(r => r.Role)
            .Include(u => u.EventSignups)
                .ThenInclude(s => s.Event)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    [Authorize(nameof(Policy.CanToggleRoles))]
    public async Task<IActionResult> ToggleRole(string id, [FromForm] string role, [FromForm] bool value)
    {
        if (await _userManager.FindByIdAsync(id) is { } user)
        {
            bool userAlreadyHasRole = await _userManager.IsInRoleAsync(user, role);

            if (value && !userAlreadyHasRole)
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            else if (!value && userAlreadyHasRole)
            {
                await _userManager.RemoveFromRoleAsync(user, role);
            }


            return RedirectToAction(nameof(Details), new { id });
        }

        return NotFound();
    }

    [HttpPost]
    [Authorize(nameof(Policy.IsAdmin))]
    public async Task<IActionResult> SetExemptions(string id, [FromForm] bool exemptFromTrainingFee, [FromForm] bool exemptFromClassesFee)
    {
        if (await _memberContext.FindAsync<User>(id) is User user)
        {
            user.ExemptFromTrainingFee = exemptFromTrainingFee;
            user.ExemptFromClassesFee = exemptFromClassesFee;

            await _memberContext.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        return NotFound();
    }

    [HttpPost]
    [Authorize(nameof(Policy.IsAdmin))]
    public async Task<IActionResult> AddManualPayment(string id, [FromForm] ManualPaymentModel model)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await _memberContext.FindAsync<User>(id) is User user)
        {
            await _memberContext.AddAsync(new Payment
            {
                User = user,
                Amount = model.Amount,
                Description = model.Description,
                IncludesMembership = model.IncludesMembership,
                IncludesTraining = model.IncludesTraining,
                IncludesClasses = model.IncludesClasses,
                PayedAtUtc = TimeProvider.UtcNow,
                ManualPayment = User.Identity.Name
            });

            await _memberContext.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePayments(string id)
    {
        if (await _memberContext.FindAsync<User>(id) is User user)
        {
            var (payments, updates) = await _paymentService.ImportPayments(user.Email);

            TempData["SuccessMessage"] = $"Fant {payments} nye betalinger, oppdaterte {updates} eksisterende betalinger";

            return RedirectToAction(nameof(Details), new { id });
        }

        return NotFound();
    }


    [HttpPost]
    [Authorize(nameof(Policy.IsAdmin))]
    public async Task<IActionResult> SendEmail([FromForm] string subject, [FromForm] string body, [FromForm] bool fromMe, [FromForm] string[] users)
    {
        var replyTo = fromMe
            ? await _memberContext.Users.SingleUser(_userManager.GetUserId(User))
            : null;
        var successes = new List<string>();
        var failures = new List<string>();

        foreach (var id in users)
        {
            var user = await _memberContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                failures.Add($"{id} is not a user");
            }
            else
            {
                try
                {
                    await _emailService.SendCustomEmail(
                        to: user,
                        subject: subject,
                        message: body,
                        replyTo: replyTo);
                    successes.Add(user.Email);
                }
                catch (Exception e)
                {
                    failures.Add($"{user.Email} - {e.Message}");
                }
            }
        }

        if (users.NotAny())
        {
            TempData["InfoMessage"] = "No emails to send";
        }

        TempData["SuccessMessage"] = string.Join(",\n ", successes);
        TempData["ErrorMessage"] = string.Join(",\n ", failures);

        var returnTo = Request.Headers["Referer"].ToString();
        if (string.IsNullOrEmpty(returnTo))
        {
            return RedirectToAction(nameof(Index));
        }
        else
        {
            return Redirect(returnTo);
        }
    }

    private static Expression<Func<User, bool>> Filter(string filter, Expression<Func<User, bool>> predicate)
    {
        switch (filter)
        {
            case "Only":
                return predicate;
            case "Not":
                return predicate.Not();
            default:
                return user => true;
        }
    }

    private static Expression<Func<User, bool>> FilterLastYear(string filter, Expression<Func<User, bool>> predicate)
    {
        switch (filter)
        {
            case "LastYear":
                return predicate;
            default:
                return user => true;
        }
    }

    private static Expression<Func<User, bool>> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return u => true;
        }

        return u => u.NameMatches(query);
    }
}
