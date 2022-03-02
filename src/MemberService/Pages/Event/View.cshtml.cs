namespace MemberService.Pages.Event;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clave.Expressionify;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Signup;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanViewEvent))]
public class ViewModel : PageModel
{
    private static readonly Status[] Statuses = {
        Status.AcceptedAndPayed,
        Status.Approved,
        Status.WaitingList,
        Status.Recommended,
        Status.Pending,
        Status.RejectedOrNotPayed,
        Status.Denied,
    };

    private readonly MemberContext _database;

    private readonly IAuthorizationService _authorizationService;

    private readonly IEmailService _emailService;

    private readonly ILoginService _linker;

    private readonly ILogger<EventController> _logger;

    public ViewModel(
        MemberContext database,
        IAuthorizationService authorizationService,
        IEmailService emailService,
        ILoginService linker,
        ILogger<EventController> logger)
    {
        _database = database;
        _authorizationService = authorizationService;
        _emailService = emailService;
        _linker = linker;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? SignedUpBefore { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Priority { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Name { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool ExcludeAcceptedElsewhere { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool ExcludeApprovedElsewhere { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool ExcludeRecommendedElsewhere { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool OnlyWaitingListElsewhere { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool OnlyRejectedElsewhere { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool OnlyDeniedElsewhere { get; set; }

    public Guid Id { get; set; }

    public Guid? SemesterId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public IReadOnlyCollection<EventSignupStatusModel> Signups { get; set; }

    public bool Archived { get; set; }

    public EventType Type { get; set; }

    public bool RoleSignup { get; set; }

    public bool AllowPartnerSignup { get; set; }

    [Required]
    [BindProperty]
    public Status Status { get; set; }

    [BindProperty]
    public List<Item> Leads { get; set; } = new();

    [BindProperty]
    public List<Item> Follows { get; set; } = new();

    [BindProperty]
    public List<Item> Solos { get; set; } = new();

    [BindProperty]
    public bool SendEmail { get; set; }

    [Required]
    [BindProperty]
    public string Subject { get; set; }

    [Required]
    [BindProperty]
    public string Message { get; set; }

    public class Item
    {
        public Guid Id { get; set; }

        public bool Selected { get; set; }
    }

    public async Task<IActionResult> OnGet(Guid id)
    {
        var model = await _database.Events
            .Include(e => e.SignupOptions)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (model is null) return NotFound();

        Id = model.Id;
        SemesterId = model.SemesterId;
        Title = model.Title;
        Description = model.Description;
        Archived = model.Archived;
        Type = model.Type;
        RoleSignup = model.SignupOptions.RoleSignup;
        AllowPartnerSignup = model.SignupOptions.AllowPartnerSignup;

        var signups = await _database.EventSignups
            .Include(s => s.User)
            .Include(s => s.AuditLog)
            .ThenInclude(l => l.User)
            .AsNoTracking()
            .Expressionify()
            .Where(e => e.EventId == id)
            .Filter(SignedUpBefore.HasValue, e => e.SignedUpAt < SignedUpBefore)
            .Filter(Priority.HasValue, e => e.Priority == Priority)
            .Filter(!string.IsNullOrWhiteSpace(Name), e => e.User.NameMatches(Name))
            .Filter(ExcludeAcceptedElsewhere || ExcludeApprovedElsewhere || ExcludeRecommendedElsewhere, e => !e.User.EventSignups
                .Where(s => s.Event.SemesterId == model.SemesterId)
                .Where(s => s.EventId != e.EventId)
                .Any(s => (ExcludeAcceptedElsewhere && s.Status == Status.AcceptedAndPayed)
                          || (ExcludeApprovedElsewhere && s.Status == Status.Approved)
                          || (ExcludeRecommendedElsewhere && s.Status == Status.Recommended)))
            .Filter(OnlyDeniedElsewhere || OnlyRejectedElsewhere || OnlyWaitingListElsewhere, e => e.User.EventSignups
                .Where(s => s.Event.SemesterId == model.SemesterId)
                .Any(s => s.EventId == e.EventId
                          || (OnlyDeniedElsewhere && s.Status == Status.Denied)
                          || (OnlyRejectedElsewhere && s.Status == Status.RejectedOrNotPayed)
                          || (OnlyWaitingListElsewhere && s.Status == Status.WaitingList)))
            .Select(signup => new
            {
                signup,
                partner = _database.Users
                    .Include(u => u.EventSignups)
                    .FirstOrDefault(u => u.NormalizedEmail == signup.PartnerEmail)
            })
            .ToListAsync();

        var signupsWithPartner = signups
            .Select(s => EventSignupModel.Create(s.signup, s.partner))
            .ToList();

        Signups = Statuses
            .Select(s => (s, signupsWithPartner.Where(x => x.Status == s).ToList()))
            .Select(EventSignupStatusModel.Create)
            .ToReadOnlyCollection();

        return Page();
    }

    public async Task<IActionResult> OnPost(Guid id)
    {
        if (!await _authorizationService.IsAuthorized(User, id, Policy.CanSetEventSignupStatus)) return Forbid();

        var currentUser = await _database.Users.SingleUser(User.GetId());

        var selected = Leads
            .Concat(Follows)
            .Concat(Solos)
            .Where(l => l.Selected)
            .Select(l => l.Id)
            .ToList();

        var statusChanged = Status != Status.Unknown;

        var failures = new List<string>();

        await _database.EditEvent(id, async eventEntry =>
        {
            foreach (var signup in selected)
            {
                var eventSignup = eventEntry.Signups.Single(s => s.Id == signup);

                if (statusChanged)
                {
                    eventSignup.Status = Status;
                }

                if (SendEmail)
                {
                    try
                    {
                        await _emailService.SendCustomEmail(
                            eventSignup.User,
                            Subject,
                            Message,
                            new(eventEntry.Title, await SignupLink(eventSignup.User, eventEntry)),
                            currentUser);

                        if (statusChanged)
                        {
                            eventSignup.AuditLog.Add($"Moved to {Status} and sent email\n\n---\n\n> {Subject}\n\n{Message}", currentUser);
                        }
                        else
                        {
                            eventSignup.AuditLog.Add($"Sent email\n\n---\n\n> {Subject}\n\n{Message}", currentUser);
                        }
                    }
                    catch (Exception e)
                    {
                        // Mail sending might fail, but that should't stop us
                        eventSignup.AuditLog.Add($"Tried to send email, but failed with message {e.Message}", currentUser);
                        _logger.LogError(e, $"Failed to send email to {eventSignup.User.Email}");
                        failures.Add($"Klarte ikke sende epost til {eventSignup.User.FullName} ({eventSignup.User.Email})");
                    }
                }
                else if (statusChanged)
                {
                    eventSignup.AuditLog.Add($"Moved to {Status} ", currentUser);
                }
            }
        });

        if (failures.Any())
        {
            TempData.SetErrorMessage(failures.JoinWithComma());
        }

        if (failures.Count != selected.Count)
        {
            TempData.SetSuccessMessage(SendEmail
                ? statusChanged
                ? $"Oppdaterte status og sendte epost til {selected.Count - failures.Count} dansere"
                : $"Sendte epost til {selected.Count - failures.Count} dansere"
                : $"Oppdaterte status på {selected.Count - failures.Count} dansere");
        }

        return RedirectToPage(new { id });
    }
    private async Task<string> SignupLink(User user, Data.Event e)
    {
        var targetLink = SignupLink(e.Id, e.Title);

        return await _linker.LoginLink(user, targetLink);
    }

    private string SignupLink(Guid id, string title) => Url.Action(
        nameof(SignupController.Event),
        "Signup",
        new
        {
            id,
            slug = title.Slugify()
        });
}
