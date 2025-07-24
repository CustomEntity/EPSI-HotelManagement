using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Payment.ValueObjects;

public sealed class RefundStatus : ValueObject
{
    public static readonly RefundStatus Pending = new("Pending");
    public static readonly RefundStatus Processing = new("Processing");
    public static readonly RefundStatus Completed = new("Completed");
    public static readonly RefundStatus Failed = new("Failed");
    public static readonly RefundStatus Cancelled = new("Cancelled");

    public string Value { get; }

    private RefundStatus(string value)
    {
        Value = value;
    }

    public bool CanBeProcessed() => 
        this == Pending;

    public bool IsCompleted() => 
        this == Completed;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}