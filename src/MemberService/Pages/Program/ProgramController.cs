using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemberService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MemberService.Pages.Program
{
    [Authorize(Roles = Roles.COORDINATOR_OR_ADMIN)]
    public class ProgramController : Controller
    {
        private readonly MemberContext _database;
        private readonly UserManager<MemberUser> _userManager;

        public ProgramController(
            MemberContext database,
            UserManager<MemberUser> userManager)
        {
            _database = database;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(bool archived = false)
        {
            var model = await _database.GetPrograms(archived);

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ProgramInputModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProgramInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var user = await GetCurrentUser();

            var id = await _database.AddProgram(input, user);

            return RedirectToAction(nameof(View), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var model = await _database.GetProgram(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _database.GetProgramInputModel(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] ProgramInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _database.EditProgram(id, e => e.UpdateProgram(model));

            return RedirectToAction(nameof(View), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> SetStatus(int id, [FromForm] string status)
        {
            await _database.EditProgram(id, p => p.SetProgramStatus(status));

            return RedirectToAction(nameof(View), new { id });
        }

        private async Task<MemberUser> GetCurrentUser()
            => await _userManager.GetUserAsync(User);
    }
}
