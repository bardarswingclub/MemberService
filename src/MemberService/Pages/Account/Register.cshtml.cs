namespace MemberService.Pages.Account;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


using MemberService.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class RegisterModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    [BindProperty]
    [Required]
    [DisplayName("Fullt navn")]
    public string FullName { get; set; }

    [BindProperty]
    [DisplayName("Tiltalsnavn")]
    public string FriendlyName { get; set; }

    [BindProperty]
    public string ReturnUrl { get; set; }

    public RegisterModel(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGetAsync(string returnUrl)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        FullName = user.FullName;
        FriendlyName = user.FriendlyName;
        ReturnUrl = returnUrl;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        user.FullName = FullName;
        user.FriendlyName = FriendlyName;

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        return Url.IsLocalUrl(ReturnUrl)
            ? Redirect(ReturnUrl)
            : Redirect("/");
    }
}
