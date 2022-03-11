namespace MemberService.Services.Vipps.Models;
using System;

public record TransactionLogEntry
{
    public int Amount { get; init; }
    public string Operation { get; init; }
    public bool OperationSuccess { get; init; }
    public string RequestId { get; init; }
    public DateTime TimeStamp { get; init; }
    public string TransactionId { get; init; }
    public string TransactionText { get; init; }
}

