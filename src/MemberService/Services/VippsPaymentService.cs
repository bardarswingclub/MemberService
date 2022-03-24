namespace MemberService.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Services.Vipps;

using Microsoft.EntityFrameworkCore;

public class VippsPaymentService : IVippsPaymentService
{
    private readonly IVippsClient _vippsClient;
    private readonly MemberContext _database;

    public VippsPaymentService(
        IVippsClient vippsClient,
        MemberContext database)
    {
        _vippsClient = vippsClient;
        _database = database;
    }

    public async Task<string> InitiatePayment(
        string userId,
        string description,
        decimal amount,
        string returnToUrl,
        bool includesMembership = false,
        bool includesTraining = false,
        bool includesClasses = false,
        Guid? eventId = null)
    {
        var reservation = new VippsReservation
        {
            UserId = userId,
            Amount = amount,
            Description = description,
            EventId = eventId,
            IncludesClasses = includesClasses,
            IncludesMembership = includesMembership,
            IncludesTraining = includesTraining,
            Secret = Guid.NewGuid().ToString()
        };

        _database.Add(reservation);
        await _database.SaveChangesAsync();

        var orderId = reservation.Id.ToString();
        var response = await _vippsClient.InitiatePayment(
            new()
            {
                Amount = (int) amount * 100,
                OrderId = orderId,
                TransactionText = description
            },
            returnToUrl
                .Replace("{orderId}", orderId)
                .Replace("%7BorderId%7D", orderId));

        return response.Url;
    }

    public async Task CapturePayment(Guid orderId, string userId, string secret = null)
    {
        var reservation = await _database.VippsReservations
            .Include(r => r.User)
            .Where(r => r.Id == orderId)
            .Where(r => r.UserId == userId)
            .FirstOrDefaultAsync();

        if (reservation is null) return;

        if (!string.IsNullOrEmpty(secret))
        {
            if (reservation.Secret != secret) return;
        }

        var response = await _vippsClient.CapturePayment(orderId.ToString(), reservation.Description);

        if (response.TransactionInfo.Status != "Captured") return;

        var payment = new Payment
        {
            VippsOrderId = orderId.ToString(),
            Amount = reservation.Amount,
            Description = reservation.Description,
            IncludesClasses = reservation.IncludesClasses,
            IncludesTraining = reservation.IncludesTraining,
            IncludesMembership = reservation.IncludesMembership,
            PayedAtUtc = response.TransactionInfo.TimeStamp,
        };

        reservation.User.Payments.Add(payment);

        if (reservation.EventId is Guid eventId)
        {
            var signup = await _database.EventSignups
                .Include(e => e.AuditLog)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.EventId == eventId);

            if (signup?.Status == Status.Approved)
            {
                signup.Status = Status.AcceptedAndPayed;
                signup.AuditLog.Add("Paid", reservation.User, payment.PayedAtUtc);
            }
        }

        _database.VippsReservations.Remove(reservation);

        await _database.SaveChangesAsync();
    }
}
