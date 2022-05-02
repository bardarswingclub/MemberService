namespace MemberService.Pages.Semester;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MemberService.Data;
using MemberService.Auth;
using Microsoft.AspNetCore.Authorization;

[Authorize(nameof(Policy.CanEditSemesterRoles))]
public class RolesModel : PageModel
{
    private readonly MemberContext _database;
    private readonly IAuthorizationService _authorizationService;

    public RolesModel(
        MemberContext database,
        IAuthorizationService authorizationService)
    {
        _database = database;
        _authorizationService = authorizationService;
    }

    public string Title { get; set; }

    public Guid Id { get; set; }

    public IReadOnlyCollection<UserRole> Users { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var semester = await _database.Semesters
            .Current(s => new
            {
                Id = s.Id,
                Title = s.Title,
                Users = s.Roles
                    .Select(r => new UserRole
                    {
                        Id = r.UserId,
                        FullName = r.User.FullName,
                        Email = r.User.Email,
                        Role = r.Role,
                        ExemptFromTrainingFee = r.User.ExemptFromTrainingFee,
                        ExemptFromClassesFee = r.User.ExemptFromClassesFee
                    })
                    .ToList()
            });

        Title = semester.Title;
        Id = semester.Id;
        Users = semester.Users;

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync([FromForm] string userId, [FromForm] SemesterRole.RoleType role)
    {
        var id = await _database.Semesters
            .Current(s => s.Id);

        _database.SemesterRoles.Add(new SemesterRole
        {
            UserId = userId,
            SemesterId = id,
            Role = role,
            UpdatedAt = DateTime.UtcNow,
            UpdatedByUser = await GetCurrentUser(),
        });

        await _database.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetExemptionAsync(
        [FromForm] string userId,
        [FromForm] bool exemptFromTrainingFee,
        [FromForm] bool exemptFromClassesFee)
    {
        if (!await _authorizationService.IsAuthorized(User, Policy.CanToggleUserFeeExemption)) return Forbid();

        var user = await _database.Users.FindAsync(userId);

        if (user is null) return NotFound();

        user.ExemptFromTrainingFee = exemptFromTrainingFee;
        user.ExemptFromClassesFee = exemptFromClassesFee;

        await _database.SaveChangesAsync();

        return new OkResult();
    }

    public async Task<IActionResult> OnPostEditAsync(
        [FromForm] string userId,
        [FromForm] SemesterRole.RoleType role,
        [FromForm] bool exemptFromTrainingFee,
        [FromForm] bool exemptFromClassesFee,
        [FromForm] bool remove)
    {
        var semesterRole = await _database.SemesterRoles
            .Include(s => s.User)
            .Where(s => s.Semester.IsActive())
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Semester.SignupOpensAt)
            .FirstOrDefaultAsync();

        if (semesterRole != null)
        {
            if (remove)
            {
                _database.SemesterRoles.Remove(semesterRole);
            }
            else
            {
                semesterRole.Role = role;
                semesterRole.UpdatedAt = DateTime.UtcNow;
                semesterRole.UpdatedByUser = await GetCurrentUser();
                semesterRole.User.ExemptFromTrainingFee = exemptFromTrainingFee;
                semesterRole.User.ExemptFromClassesFee = exemptFromClassesFee;
            }

            await _database.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetUsersAsync(string q)
    {
        var id = await _database.Semesters
            .Current(s => s.Id);

        var model = await _database.Users
            .Except(_database.SemesterRoles
                .Where(o => o.SemesterId == id)
                .Select(o => o.User))
            .Where(u => u.NameMatches(q))
            .Select(u => new
            {
                value = u.Id,
                text = u.FullName + " (" + u.Email + ")"
            })
            .Take(10)
            .ToListAsync();

        return new JsonResult(model);
    }

    private async Task<User> GetCurrentUser() => await _database.Get(User);

    public class UserRole
    {
        public string Id { get; init; }
        public string FullName { get; init; }
        public string Email { get; init; }
        public SemesterRole.RoleType Role { get; init; }
        public bool ExemptFromTrainingFee { get; init; }
        public bool ExemptFromClassesFee { get; init; }
    }
}
