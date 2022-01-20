namespace MemberService.Pages.AnnualMeeting;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using MemberService.Auth;
using MemberService.Data;
using MemberService.Pages.Event;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(nameof(Policy.CanCreateAnnualMeeting))]
[BindProperties]
public class CreateModel : PageModel
{
    private readonly MemberContext _database;

    public CreateModel(MemberContext database)
    {
        _database = database;
    }

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

    public IActionResult OnGet()
    {
        var (startDate, startTime) = TimeProvider.UtcNow.GetLocalDateAndTime();

        MeetingStartsAtDate = startDate;
        MeetingStartsAtTime = startTime;
        MeetingEndsAtDate = startDate;
        MeetingEndsAtTime = "23:59";

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _database.AnnualMeetings.Add(new AnnualMeeting
        {
            MeetingStartsAt = MeetingStartsAtDate.GetUtc(MeetingStartsAtTime),
            MeetingEndsAt = MeetingEndsAtDate.GetUtc(MeetingEndsAtTime),
            Title = Title,
            MeetingInvitation = Invitation,
            MeetingInfo = Info,
            MeetingSummary = Summary
        });

        await _database.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
