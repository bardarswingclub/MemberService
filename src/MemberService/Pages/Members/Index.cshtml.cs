namespace MemberService.Pages.Members;

using System.Linq.Expressions;
using System.Text;

using Clave.Expressionify;
using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Pages.Shared;
using MemberService.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanViewMembers))]
public class IndexModel : PageModel
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly IAuthorizationService _authorizationService;

    public IndexModel(
        MemberContext Database,
        UserManager<User> userManager,
        IEmailService emailService,
        IAuthorizationService authorizationService)
    {
        _database = Database;
        _userManager = userManager;
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
        => await _database.Users
            .Expressionify()
            .Where(u => u.EmailConfirmed)
            .Where(FilterMembership(MemberFilter))
            .Where(Filter(TrainingFilter, u => u.HasPayedTrainingFeeThisSemester()))
            .Where(Filter(ClassesFilter, u => u.HasPayedClassesFeeThisSemester()))
            .Where(Filter(ExemptTrainingFilter, u => u.ExemptFromTrainingFee))
            .Where(Filter(ExemptClassesFilter, u => u.ExemptFromClassesFee))
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

    public async Task<IActionResult> OnPostSendEmail([FromForm] string subject, [FromForm] string body, [FromForm] bool fromMe, [FromForm] string[] users)
    {
        if (!await _authorizationService.IsAuthorized(User, Policy.CanSendEmailToMembers)) return Forbid();

        var replyTo = fromMe
            ? await _database.Users.SingleUser(_userManager.GetUserId(User))
            : null;
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
            TempData["InfoMessage"] = "No emails to send";
        }

        TempData["SuccessMessage"] = successes.Any() ? $"Epost sent til disse {successes.Count}: {successes.JoinWithComma()}" : null;
        TempData["ErrorMessage"] = failures.Any() ? $"Kunne ikke sende epost til disse {failures.Count}: {failures.JoinWithComma()}" : null;

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

    private static Expression<Func<User, bool>> FilterMembership(string filter)
        => filter switch
        {
            "LastYear" => u => u.HasPayedMembershipLastYear(),
            "LastOrThisYear" => u => u.HasPayedMembershipLastOrThisYear(),
            var f => Filter(f, u => u.HasPayedMembershipThisYear())
        };

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
