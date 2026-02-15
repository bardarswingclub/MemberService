namespace MemberService.Data;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class EventCommunication
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public Event Event { get; set; }

    [Required]
    public string SentByUserId { get; set; }

    [ForeignKey(nameof(SentByUserId))]
    public User SentByUser { get; set; }

    [Required]
    public string Subject { get; set; }

    [Required]
    public string Message { get; set; }

    public DateTime SentAtUtc { get; set; }

    public ICollection<EventCommunicationRecipient> Recipients { get; set; } = new List<EventCommunicationRecipient>();
}

public class EventCommunicationRecipient
{
    public Guid Id { get; set; }

    public Guid EventCommunicationId { get; set; }

    public EventCommunication EventCommunication { get; set; }

    [Required]
    public string RecipientUserId { get; set; }

    [ForeignKey(nameof(RecipientUserId))]
    public User RecipientUser { get; set; }

    public bool Success { get; set; }
}
