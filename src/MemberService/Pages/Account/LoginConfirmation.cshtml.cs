namespace MemberService.Pages.Account;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class LoginConfirmationModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public bool ShowError { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [DisplayName("E-post")]
        public string Email { get; set; }

        [Required]
        [DisplayName("Kode")]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }
    }

    public IActionResult OnGet(string email, string returnUrl, bool showError)
    {
        if (email == null)
        {
            return RedirectToAction("Index", "Home");
        }

        ShowError = showError;

        Input = new InputModel
        {
            Email = email,
            ReturnUrl = returnUrl
        };

        return Page();
    }
}
