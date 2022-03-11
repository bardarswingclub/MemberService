namespace MemberService.Data;

using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

public class VippsReservation
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; }

    public User User { get; set; }

    [Required]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; }

    public bool IncludesMembership { get; set; }

    public bool IncludesTraining { get; set; }

    public bool IncludesClasses { get; set; }

    public Guid? EventId { get; set; }

    public string Secret { get; set; }
}