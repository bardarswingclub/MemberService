﻿namespace MemberService.Data;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using MemberService.Data.ValueTypes;

public class EventSignup
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }

    public Event Event { get; set; }

    [Required]
    public string UserId { get; set; }

    public User User { get; set; }

    [Required]
    public DateTime SignedUpAt { get; set; }

    public DanceRole Role { get; set; }

    public string PartnerEmail { get; set; }

    public int Priority { get; set; }

    public Status Status { get; set; }

    public string PaymentId { get; set; }

    [ForeignKey(nameof(PaymentId))]
    public Payment Payment { get; set; }

    public ICollection<EventSignupAuditEntry> AuditLog { get; set; } = new List<EventSignupAuditEntry>();

    public ICollection<Presence> Presence { get; set; } = new List<Presence>();

    public Guid? ResponseId { get; set; }

    public Response Response { get; set; }
}
