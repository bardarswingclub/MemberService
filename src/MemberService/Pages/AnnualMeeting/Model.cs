namespace MemberService.Pages.AnnualMeeting;




using MemberService.Pages.AnnualMeeting.Survey;

public class Model
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string MeetingInvitation { get; set; }

    public string MeetingInfo { get; set; }

    public string MeetingSummary { get; set; }

    public bool IsMember { get; set; }

    public DateTime MeetingStartsAt { get; set; }

    public bool HasStarted { get; set; }

    public bool HasEnded { get; set; }

    public IReadOnlyList<Attendee> Attendees { get; set; }

    public SurveyResultModel VotingResults { get; set; }
    public string UserId { get; set; }

    public class Attendee
    {
        public string UserId { get; set; }

        public string Name { get; set; }

        public DateTime FirstVisit { get; set; }

        public DateTime LastVisit { get; set; }

        public int Visits { get; set; }
    }
}
