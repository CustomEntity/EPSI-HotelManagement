using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Payment.ValueObjects;

public sealed class PaymentStatus : ValueObject
{
    public static readonly PaymentStatus Pending = new("Pending");
    public static readonly PaymentStatus Processing = new("Processing");
    public static readonly PaymentStatus Completed = new("Completed");
    public static readonly PaymentStatus Failed = new("Failed");
    public static readonly PaymentStatus Refunded = new("Refunded");
    public static readonly PaymentStatus PartiallyRefunded = new("PartiallyRefunded");
    public static readonly PaymentStatus Cancelled = new("Cancelled");

    public string Value { get; }

    private PaymentStatus(string value)
    {
        Value = value;
    }

    public bool CanBeProcessed() => 
        this == Pending;

    public bool CanBeRefunded() => 
        this == Completed || this == PartiallyRefunded;

    public bool IsSuccessful() => 
        this == Completed || this == PartiallyRefunded || this == Refunded;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}