namespace MemberService.Pages.Members;

using System.Linq.Expressions;

using Clave.Expressionify;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Pages.Shared;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanViewMembers))]
public class IndexModel : PageModel
{
    private readonly MemberContext _database;
    private readonly IEmailService _emailService;
    private readonly IAuthorizationService _authorizationService;

    public IndexModel(
        MemberContext Database,
        IEmailService emailService,
        IAuthorizationService authorizationService)
    {
        _database = Database;
        _emailService = emailService;
        _authorizationService = authorizationService;
    }

    public IReadOnlyCollection<(char Letter, IReadOnlyCollection<Member> Users)> Users { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Query { get; set; }

    [BindProperty(SupportsGet = true)]
    public string MemberFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string TrainingFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ClassesFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ExemptTrainingFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ExemptClassesFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Role { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var users = await GetFilteredUsers();

        Users = users
            .GroupBy(u => u.FullName?.ToUpper().FirstOrDefault() ?? '?', (key, u) => (key, u.ToReadOnlyCollection()))
            .ToReadOnlyCollection();

        return Page();
    }

    public async Task<IActionResult> OnGetExport()
    {
        var rows = await GetFilteredUsers();

        return CsvResult.Create(rows, "members.csv");
    }

    private async Task<List<Member>> GetFilteredUsers()
    {
        var canToggleRoles = await _authorizationService.IsAuthorized(User, Policy.CanToggleRoles);
        var canViewOlderMembers = await _authorizationService.IsAuthorized(User, Policy.CanViewOlderMembers);

        return await _database.Users
            .Expressionify()
            .Where(u => u.EmailConfirmed)
            .Where(FilterMembership(MemberFilter, canViewOlderMembers))
            .Where(Filter(TrainingFilter, u => u.HasPayedTrainingFeeThisSemester()))
            .Where(Filter(ClassesFilter, u => u.HasPayedClassesFeeThisSemester()))
            .Where(Filter(ExemptTrainingFilter, u => u.ExemptFromTrainingFee))
            .Where(Filter(ExemptClassesFilter, u => u.ExemptFromClassesFee))
            .Where(FilterRole(Role, canToggleRoles))
            .Where(Search(Query))
            .OrderBy(u => u.FullName)
            .Select(u => new Member
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                HasPayedMembershipThisYear = u.HasPayedMembershipThisYear(),
                HasPayedTrainingFeeThisSemester = u.HasPayedTrainingFeeThisSemester(),
                HasPayedClassesFeeThisSemester = u.HasPayedClassesFeeThisSemester(),
                ExemptFromTrainingFee = u.ExemptFromTrainingFee,
                ExemptFromClassesFee = u.ExemptFromClassesFee,
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostSendEmail([FromForm] string subject, [FromForm] string body, [FromForm] string[] users)
    {
        if (!await _authorizationService.IsAuthorized(User, Policy.CanSendEmailToMembers)) return Forbid();

        var replyTo = await _database.Users.SingleUser(User.GetId());
        var successes = new List<string>();
        var failures = new List<string>();

        foreach (var id in users)
        {
            var user = await _database.Users.FirstOrDefaultAsync(u => u.Id == id);
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
            TempData.SetInfoMessage("No emails to send");
        }

        if (successes.Any())
        {
            TempData.SetSuccessMessage($"Epost sent til disse {successes.Count}: {successes.JoinWithComma()}");
        }

        if (failures.Any())
        {
            TempData.SetErrorMessage($"Kunne ikke sende epost til disse {failures.Count}: {failures.JoinWithComma()}");
        }

        var returnTo = Request.Headers["Referer"].ToString();
        if (string.IsNullOrEmpty(returnTo))
        {
            return RedirectToPage();
        }
        else
        {
            return Redirect(returnTo);
        }
    }

    private static Expression<Func<User, bool>> Filter(string filter, Expression<Func<User, bool>> predicate)
        => filter switch
        {
            "Only" => predicate,
            "Not" => predicate.Not(),
            _ => user => true,
        };

    private static Expression<Func<User, bool>> FilterMembership(string filter, bool canViewOlderMembers)
        => filter switch
        {
            "LastYear" when canViewOlderMembers => u => u.HasPayedMembershipLastYear(),
            "LastOrThisYear" when canViewOlderMembers => u => u.HasPayedMembershipLastOrThisYear(),
            var f => Filter(f, u => u.HasPayedMembershipThisYear())
        };

    private static Expression<Func<User, bool>> FilterRole(string filter, bool canToggleRoles)
        => Roles.All.Contains(filter) && canToggleRoles
            ? user => user.UserRoles.Any(r => r.Role.Name == filter)
            : user => true;

    private static Expression<Func<User, bool>> Search(string query)
        => string.IsNullOrWhiteSpace(query)
            ? (u => true)
            : (u => u.NameMatches(query));

    public class Member
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool HasPayedMembershipThisYear { get; set; }
        public bool HasPayedTrainingFeeThisSemester { get; set; }
        public bool HasPayedClassesFeeThisSemester { get; set; }
        public bool ExemptFromTrainingFee { get; set; }
        public bool ExemptFromClassesFee { get; set; }
    }
}
