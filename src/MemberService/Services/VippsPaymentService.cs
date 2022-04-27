namespace MemberService.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

using MemberService.Data;
using MemberService.Data.ValueTypes;
using MemberService.Services.Vipps;
using MemberService.Services.Vipps.Models;

using Microsoft.EntityFrameworkCore;

public class VippsPaymentService : IVippsPaymentService
{
    private readonly IVippsClient _vippsClient;
    private readonly MemberContext _database;
    private readonly ILogger<VippsPaymentService> _logger;

    public VippsPaymentService(
        IVippsClient vippsClient,
        MemberContext database,
        ILogger<VippsPaymentService> logger)
    {
        _vippsClient = vippsClient;
        _database = database;
        _logger = logger;
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
                TransactionText = description,
            },
            reservation.Secret,
            returnToUrl
                .Replace("{orderId}", orderId)
                .Replace("%7BorderId%7D", orderId));

        return response.Url;
    }

    public async Task CompletePayment(Guid orderId, string secret)
    {
        var reservation = await _database.VippsReservations
            .Include(r => r.User)
            .Where(r => r.Id == orderId)
            .FirstOrDefaultAsync();

        if (reservation is null) return;

        if (reservation.Secret != secret) return;

        await CompletePayment(reservation);

        try
        {
            await _database.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency issue");
        }
    }

    public async Task<bool> CompleteReservations(string userId)
    {
        var reservations = await _database.VippsReservations
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        var result = false;
        foreach (var reservation in reservations)
        {
            result |= await CompletePayment(reservation);
        }

        try
        {
            await _database.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency issue");
        }

        return result;
    }

    private async Task<bool> CompletePayment(VippsReservation reservation)
    {
        var success = false;
        var paymentDetails = await GetPaymentDetails(reservation);

        if (paymentDetails?.TransactionSummary?.RemainingAmountToCapture > 0)
        {
            success = await Capture(reservation);
        }

        _database.VippsReservations.Remove(reservation);

        return success;
    }

    private async Task<PaymentDetails> GetPaymentDetails(VippsReservation reservation)
    {
        try
        {
            return await _vippsClient.GetPaymentDetails(reservation.Id.ToString());
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Failed to get payment details");
            return null;
        }
    }

    private async Task<bool> Capture(VippsReservation reservation)
    {
        var response = await CapturePayment(reservation);

        if (response?.TransactionInfo.Status == "Captured")
        {
            var payment = new Payment
            {
                VippsOrderId = reservation.Id.ToString(),
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
                    .FirstOrDefaultAsync(e => e.UserId == reservation.UserId && e.EventId == eventId);

                if (signup?.Status == Status.Approved)
                {
                    signup.Status = Status.AcceptedAndPayed;
                    signup.AuditLog.Add("Paid", reservation.User, payment.PayedAtUtc);
                }
            }

            return true;
        }

        return false;
    }

    private async Task<CapturePaymentResponse> CapturePayment(VippsReservation reservation)
    {
        try
        {
            return await _vippsClient.CapturePayment(reservation.Id.ToString(), reservation.Description);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Failed to capture payment");
            return null;
        }
    }
}
