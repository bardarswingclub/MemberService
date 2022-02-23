namespace MemberService.Pages.Members;

using System.Collections.Generic;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanViewMembers))]
public class DetailsModel : PageModel
{
    private readonly MemberContext _memberContext;
    private readonly UserManager<User> _userManager;
    private readonly IPaymentService _paymentService;
    private readonly IAuthorizationService _authorizationService;

    public DetailsModel(
        MemberContext memberContext,
        UserManager<User> userManager,
        IPaymentService paymentService,
        IAuthorizationService authorizationService)
    {
        _memberContext = memberContext;
        _userManager = userManager;
        _paymentService = paymentService;
        _authorizationService = authorizationService;
    }

    public string Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public IReadOnlyCollection<Payment> Payments { get; set; }
    public IReadOnlyCollection<EventSignup> EventSignups { get; set; }
    public bool HasPayedMembershipThisYear { get; private set; }
    public bool HasPayedTrainingFeeThisSemester { get; private set; }
    public bool HasPayedClassesFeeThisSemester { get; private set; }
    [BindProperty]
    public bool ExemptFromTrainingFee { get; set; }
    [BindProperty]
    public bool ExemptFromClassesFee { get; set; }
    public IReadOnlyList<string> Roles { get; private set; }

    public async Task<IActionResult> OnGet(string id)
    {
        var user = await _memberContext.Users
            .Include(u => u.Payments)
            .Include(u => u.UserRoles)
                .ThenInclude(r => r.Role)
            .Include(u => u.EventSignups)
                .ThenInclude(s => s.Event)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        Id = user.Id;
        FullName = user.FullName;
        Email = user.Email;
        Payments = user.Payments.ToList();
        EventSignups = user.EventSignups.ToList();
        HasPayedMembershipThisYear = user.HasPayedMembershipThisYear();
        HasPayedTrainingFeeThisSemester = user.HasPayedTrainingFeeThisSemester();
        HasPayedClassesFeeThisSemester = user.HasPayedClassesFeeThisSemester();
        ExemptFromClassesFee = user.ExemptFromClassesFee;
        ExemptFromTrainingFee = user.ExemptFromTrainingFee;
        Roles = user.UserRoles.Select(r => r.Role.Name).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostToggleRole(string id, [FromForm] string role, [FromForm] bool value)
    {
        if (!await _authorizationService.IsAuthorized(User, Policy.CanToggleRoles)) return Forbid();

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


            return RedirectToPage(new { id });
        }

        return NotFound();
    }

    public async Task<IActionResult> OnPostSetExemptions(string id)
    {
        if (!await _authorizationService.IsAuthorized(User, Policy.CanToggleUserFeeExemption)) return Forbid();

        if (await _memberContext.Users.FindAsync(id) is not User user) return NotFound();

        user.ExemptFromTrainingFee = ExemptFromTrainingFee;
        user.ExemptFromClassesFee = ExemptFromClassesFee;

        await _memberContext.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddManualPayment(string id, [FromForm] ManualPaymentModel model)
    {
        if (!await _authorizationService.IsAuthorized(User, Policy.CanAddManualPayment)) return Forbid();

        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await _memberContext.Users.FindAsync(id) is not User user) return NotFound();

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

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUpdatePayments(string id)
    {
        if (!await _authorizationService.IsAuthorized(User, Policy.CanUpdatePayments)) return Forbid();

        if (await _memberContext.Users.FindAsync(id) is not User user) return NotFound();

        var (payments, updates) = await _paymentService.ImportPayments(user.Email);

        TempData.SetSuccessMessage($"Fant {payments} nye betalinger, oppdaterte {updates} eksisterende betalinger");

        return RedirectToPage(new { id });
    }
}
