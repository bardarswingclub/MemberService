namespace MemberService.Pages.AnnualMeeting;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Pages.Event;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(nameof(Policy.CanEditAnnualMeeting))]
[BindProperties]
public class EditModel : PageModel
{
    private readonly MemberContext _database;

    public EditModel(MemberContext database)
    {
        _database = database;
    }

    public Guid Id { get; set; }

    [Required]
    [DisplayName("Navn")]
    public string Title { get; set; }

    [Required]
    [DisplayName("Invitasjonstekst")]
    public string Invitation { get; set; }

    [Required]
    [DisplayName("Møteinfo")]
    public string Info { get; set; }

    [Required]
    [DisplayName("Referat")]
    public string Summary { get; set; }

    [Required]
    [DisplayName("Møtet starter")]
    public string MeetingStartsAtDate { get; set; }

    [Required]
    [RegularExpression(@"^\d\d:\d\d$")]
    public string MeetingStartsAtTime { get; set; }

    [Required]
    [DisplayName("Møtet slutter")]
    public string MeetingEndsAtDate { get; set; }

    [Required]
    [RegularExpression(@"^\d\d:\d\d$")]
    public string MeetingEndsAtTime { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var model = await _database.AnnualMeetings.FindAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        var (startDate, startTime) = model.MeetingStartsAt.GetLocalDateAndTime();
        var (endDate, endTime) = model.MeetingEndsAt.GetLocalDateAndTime();

        Id = model.Id;
        Title = model.Title;
        Invitation = model.MeetingInvitation;
        Info = model.MeetingInfo;
        Summary = model.MeetingSummary;
        MeetingStartsAtDate = startDate;
        MeetingStartsAtTime = startTime;
        MeetingEndsAtDate = endDate;
        MeetingEndsAtTime = endTime;

        return Page();
    }

    public async Task<IActionResult> OnPost(Guid id, [FromForm] string submit)
    {
        if (!ModelState.IsValid)
        {
            Id = id;
            return Page();
        }

        var model = await _database.AnnualMeetings.FindAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        if (submit == "EndMeeting")
        {
            model.MeetingEndsAt = TimeProvider.UtcNow;
        }
        else
        {
            model.MeetingStartsAt = MeetingStartsAtDate.GetUtc(MeetingStartsAtTime);
            model.MeetingEndsAt = MeetingEndsAtDate.GetUtc(MeetingEndsAtTime);
            model.Title = Title;
            model.MeetingInvitation = Invitation;
            model.MeetingInfo = Info;
            model.MeetingSummary = Summary;

            model.Survey ??= new Data.Survey();
        }

        await _database.SaveChangesAsync();

        return RedirectToPage(nameof(Index));
    }
}
