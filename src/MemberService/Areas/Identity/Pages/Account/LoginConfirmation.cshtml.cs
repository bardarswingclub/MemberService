using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MemberService.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginConfirmationModel : PageModel
    {
        private readonly UserManager<MemberUser> _userManager;
        private readonly SignInManager<MemberUser> _signInManager;

        public LoginConfirmationModel(
            UserManager<MemberUser> userManager,
            SignInManager<MemberUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

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
}
