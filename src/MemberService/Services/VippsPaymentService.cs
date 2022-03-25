﻿namespace MemberService.Services;
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
    }

    public async Task<bool> CompleteReservations(string userId)
    {
        var reservations = await _database.VippsReservations
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        var result = true;
        foreach (var reservation in reservations)
        {
            result &= await CompletePayment(reservation);
        }

        return result;
    }

    private async Task<bool> CompletePayment(VippsReservation reservation)
    {
        var paymentDetails = await _vippsClient.GetPaymentDetails(reservation.Id.ToString());

        var success = false;
        if (paymentDetails.TransactionSummary?.RemainingAmountToCapture > 0)
        {
            success = await Capture(reservation);
        }

        _database.VippsReservations.Remove(reservation);

        await _database.SaveChangesAsync();

        return success;
    }

    private async Task<bool> Capture(VippsReservation reservation)
    {
        var response = await _vippsClient.CapturePayment(reservation.Id.ToString(), reservation.Description);

        if (response.TransactionInfo.Status == "Captured")
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
}
