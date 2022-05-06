namespace MemberService.Data;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class Payment : IEntityTypeConfiguration<Payment>
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }

    [Required]
    public User User { get; set; }

    [Required]
    public DateTime PayedAtUtc { get; set; }

    public string StripeChargeId { get; set; }

    public string VippsOrderId { get; set; }

    public string ManualPayment { get; set; }

    [Required]
    [Precision(18, 2)]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; }

    public bool IncludesMembership { get; set; }

    public bool IncludesTraining { get; set; }

    public bool IncludesClasses { get; set; }

    [Precision(18, 2)]
    public decimal RefundedAmount { get; set; }

    [InverseProperty(nameof(Data.EventSignup.Payment))]
    public EventSignup EventSignup { get; set; }

    public void Configure(EntityTypeBuilder<Payment> payment)
        => payment.HasIndex(p => p.PayedAtUtc);
}
