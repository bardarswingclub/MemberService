namespace MemberService.Pages.Semester;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MemberService.Data;
using Clave.Expressionify;
using MemberService.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

[Authorize(nameof(Policy.CanEditSemesterRoles))]
public class RolesModel : PageModel
{
    private readonly MemberContext _database;
    private readonly UserManager<User> _userManager;

    public RolesModel(
        MemberContext database,
        UserManager<User> userManager)
    {
        _database = database;
        _userManager = userManager;
    }

    public string Title { get; set; }

    public Guid Id { get; set; }

    public IReadOnlyCollection<UserRole> Users { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var semester = await _database.Semesters
            .Expressionify()
            .Where(s => s.IsActive())
            .OrderByDescending(s => s.SignupOpensAt)
            .Select(s => new 
            {
                Id = s.Id,
                Title = s.Title,
                Users = s.Roles
                    .Select(r => new UserRole
                    {
                        Id = r.UserId,
                        FullName = r.User.FullName,
                        Email = r.User.Email,
                        Role = r.Role
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        Title = semester.Title;
        Id = semester.Id;
        Users = semester.Users;

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync([FromForm] string userId, [FromForm] SemesterRole.RoleType role)
    {
        var id = await _database.Semesters
            .Expressionify()
            .Where(s => s.IsActive())
            .OrderByDescending(s => s.SignupOpensAt)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

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

    public async Task<IActionResult> OnPostEditAsync([FromForm] string userId, [FromForm] SemesterRole.RoleType role, [FromForm] bool remove)
    {
        var user = await _database.SemesterRoles
            .Expressionify()
            .Where(s => s.Semester.IsActive())
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Semester.SignupOpensAt)
            .FirstOrDefaultAsync();

        if (user != null)
        {
            if (remove)
            {
                _database.SemesterRoles.Remove(user);
            }
            else
            {
                user.Role = role;
                user.UpdatedAt = DateTime.UtcNow;
                user.UpdatedByUser = await GetCurrentUser();
            }

            await _database.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetUsersAsync(string q)
    {
        var id = await _database.Semesters
            .Expressionify()
            .Where(s => s.IsActive())
            .OrderByDescending(s => s.SignupOpensAt)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        var model = await _database.Users
            .Expressionify()
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
    private async Task<User> GetCurrentUser()
        => await _database.Users.SingleUser(_userManager.GetUserId(User));

    public class UserRole
    {
        public string Id { get; init; }

        public string FullName { get; init; }

        public string Email { get; init; }

        public SemesterRole.RoleType Role { get; init; }
    }
}
