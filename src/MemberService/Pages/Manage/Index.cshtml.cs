namespace MemberService.Pages.Manage;


using System.ComponentModel;
using System.ComponentModel.DataAnnotations;



using Clave.ExtensionMethods;

using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class AddConsentModel
{
    [Required(ErrorMessage = "Du må ta et valg om samtykke.")]
    [DisplayName("Samtykke")]
    public SomeConsentState? State { get; set; }
}

public class UpdateUserModel
{
    [BindProperty]
    [DisplayName("Fullt navn")]
    public string FullName { get; set; }

    [BindProperty]
    [DisplayName("Tiltalsnavn")]
    public string FriendlyName { get; set; }
    
}

[Authorize]
public partial class IndexModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly MemberContext _memberContext;
    private readonly SignInManager<User> _signInManager;

    public IndexModel(
        UserManager<User> userManager,
        MemberContext memberContext,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _memberContext = memberContext;
        _signInManager = signInManager;
    }

    public string UserId { get; set; }

    [DisplayName("E-post")]
    public string Email { get; set; }

    public UpdateUserModel UpdateUser { get; set; } = new();
    public AddConsentModel NewConsent { get; set; } = new();

    public IReadOnlyCollection<Payment> Payments { get; private set; }

    public IReadOnlyCollection<SignupModel> EventSignups { get; private set; }
    public IReadOnlyCollection<SomeConsentRecord> ConsentRecords{ get; private set; }

    [TempData]
    public string SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        UserId = User.GetId();

        var user = await _memberContext.Users
            .Include(x => x.Payments)
            .Include(u => u.ConsentRecords)
            .Include(x => x.EventSignups.Where(e => !e.Event.SemesterId.HasValue))
                .ThenInclude(s => s.Event)
            .SingleUser(UserId);

        if (user == null)
        {
            return base.NotFound($"Unable to load user with ID '{UserId}'.");
        }

        Email = user.Email;

        Payments = user.Payments
            .OrderByDescending(p => p.PayedAtUtc)
            .ToList();

        EventSignups = user.EventSignups
            .OrderByDescending(s => s.SignedUpAt)
            .Select(SignupModel.Create)
            .ToReadOnlyCollection();

        ConsentRecords = user.ConsentRecords
            .OrderByDescending(p => p.ChangedAtUtc)
            .ToReadOnlyCollection();

        UpdateUser.FullName = user.FullName;
        UpdateUser.FriendlyName = user.FriendlyName;

        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync([FromForm] UpdateUserModel updateUser)
    {
        if (string.IsNullOrWhiteSpace(updateUser.FullName))
        {
            // Add a model validation error
            ModelState.AddModelError(nameof(updateUser.FullName), "Fullt navn kan ikke være tomt");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{User.GetId()}'.");
        }

        user.FullName = updateUser.FullName;
        user.FriendlyName = updateUser.FriendlyName;

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        SuccessMessage = "Navnet ditt har blitt lagret :)";
        return RedirectToPage();
    }

    public class SignupModel
    {
        public Guid EventId { get; init; }
        public string Title { get; init; }
        public Status Status { get; init; }
        public DateTime SignedUpAt { get; init; }
        public DanceRole Role { get; init; }

        public static SignupModel Create(EventSignup s)
        {
            return new()
            {
                EventId = s.EventId,
                Title = s.Event.Title,
                Status = s.Status,
                SignedUpAt = s.SignedUpAt,
                Role = s.Role
            };
        }
    }

    public async Task<IActionResult> OnPostAddConsentAsync([FromForm] AddConsentModel newConsent)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("User not found");

        var record = new SomeConsentRecord(newConsent.State!.Value, user.Id);

        _memberContext.SomeConsentRecords.Add(record);
        await _memberContext.SaveChangesAsync();

        SuccessMessage = "Samtykket dit er oppdatert";
        return RedirectToPage(); // reload page so list updates
    }
}
