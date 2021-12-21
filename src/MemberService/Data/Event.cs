namespace MemberService.Data;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using MemberService.Data.ValueTypes;

public class Event
{
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public User CreatedByUser { get; set; }

    public EventSignupOptions SignupOptions { get; set; } = new();

    public Guid? SurveyId { get; set; }

    public Survey Survey { get; set; }

    public Guid? SemesterId { get; set; }

    public Semester Semester { get; set; }

    public ICollection<EventSignup> Signups { get; set; } = new List<EventSignup>();

    public ICollection<EventOrganizer> Organizers { get; set; } = new List<EventOrganizer>();

    public bool Published { get; set; }

    public bool Archived { get; set; }

    public bool Cancelled { get; set; }

    public EventType Type { get; set; }

    public int LessonCount { get; set; }
}
