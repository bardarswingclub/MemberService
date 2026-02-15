namespace MemberService.Pages.Event;

using MemberService.Auth;
using MemberService.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanViewEvent))]
public class CommunicationModel : PageModel
{
    private readonly MemberContext _database;

    public CommunicationModel(MemberContext database)
    {
        _database = database;
    }

    public Guid EventId { get; set; }

    public string Title { get; set; }

    public IReadOnlyList<CommunicationEntry> Communications { get; set; }

    public class CommunicationEntry
    {
        public DateTime SentAtUtc { get; set; }

        public string SentByName { get; set; }

        public string Subject { get; set; }

        public string Message { get; set; }

        public IReadOnlyList<RecipientEntry> Recipients { get; set; }
    }

    public class RecipientEntry
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public bool Success { get; set; }
    }

    public async Task<IActionResult> OnGet(Guid id)
    {
        var ev = await _database.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev is null) return NotFound();

        EventId = ev.Id;
        Title = ev.Title;

        Communications = await _database.EventCommunications
            .Where(c => c.EventId == id)
            .OrderByDescending(c => c.SentAtUtc)
            .Select(c => new CommunicationEntry
            {
                SentAtUtc = c.SentAtUtc,
                SentByName = c.SentByUser.FullName,
                Subject = c.Subject,
                Message = c.Message,
                Recipients = c.Recipients
                    .Select(r => new RecipientEntry
                    {
                        Name = r.RecipientUser.FullName,
                        Email = r.RecipientUser.Email,
                        Success = r.Success
                    })
                    .ToList()
            })
            .AsNoTracking()
            .ToListAsync();

        return Page();
    }
}
