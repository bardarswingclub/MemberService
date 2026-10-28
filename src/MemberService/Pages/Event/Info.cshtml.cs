namespace MemberService.Pages.Event;

using Clave.ExtensionMethods;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Data.ValueTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

[Authorize(nameof(Policy.CanViewEvent))]
public class InfoModel : PageModel
{
    private readonly MemberContext _database;

    public InfoModel(MemberContext database)
    {
        _database = database;
    }

    public Guid EventId { get; set; }

    public string Title { get; set; }

    public IReadOnlyCollection<ParticipantInfo> Participants { get; set; }

    public IReadOnlyCollection<OriginCount> Origins { get; set; }

    public class ParticipantInfo
    {
        public string UserId { get; set; }

        public string FriendlyName { get; set; }

        public string FullName { get; set; }

        public Status Status { get; set; }

        public string Origin { get; set; }

        public string SpecialNeeds { get; set; }
    }

    public record OriginCount(string Origin, int Count);

    public async Task<IActionResult> OnGet(Guid id)
    {
        var ev = await _database.Events
            .Include(e => e.Signups)
                .ThenInclude(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev is null) return NotFound();

        EventId = ev.Id;
        Title = ev.Title;

        Participants = ev.Signups
            .Where(s => s.Status == Status.AcceptedAndPayed || s.Status == Status.Approved)
            .Select(s => new ParticipantInfo
            {
                UserId = s.UserId,
                FriendlyName = s.User.GetFriendlyName(),
                FullName = s.User.FullName,
                Status = s.Status,
                Origin = s.Origin,
                SpecialNeeds = s.SpecialNeeds
            })
            .OrderBy(p => p.FullName)
            .ToReadOnlyCollection();

        Origins = Participants
            .Where(p => !string.IsNullOrWhiteSpace(p.Origin))
            .GroupBy(p => p.Origin.Trim(), StringComparer.CurrentCultureIgnoreCase)
            .Select(g => new OriginCount(g.Key, g.Count()))
            .OrderByDescending(o => o.Count)
            .ThenBy(o => o.Origin)
            .ToReadOnlyCollection();

        return Page();
    }
}
