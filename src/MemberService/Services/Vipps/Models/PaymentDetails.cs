namespace MemberService.Services.Vipps.Models;

public record PaymentDetails
{
    public string OrderId { get; init; }
    //public Shippingdetails shippingDetails { get; init; }
    public IReadOnlyList<TransactionLogEntry> TransactionLogHistory { get; init; }
    public TransactionSummary TransactionSummary { get; init; }
    //public Userdetails userDetails { get; init; }
    public string Sub { get; init; }
}


/*
public record Shippingdetails
{
    public Address address { get; init; }
    public int shippingCost { get; init; }
    public string shippingMethod { get; init; }
    public string shippingMethodId { get; init; }
}

public record Address
{
    public string addressLine1 { get; init; }
    public string addressLine2 { get; init; }
    public string city { get; init; }
    public string country { get; init; }
    public string postCode { get; init; }
}


public record Userdetails
{
    public string bankIdVerified { get; init; }
    public string dateOfBirth { get; init; }
    public string email { get; init; }
    public string firstName { get; init; }
    public string lastName { get; init; }
    public string mobileNumber { get; init; }
    public string ssn { get; init; }
    public string userId { get; init; }
}
*/