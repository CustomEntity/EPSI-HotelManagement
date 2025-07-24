using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Customer.ValueObjects;

public sealed class CustomerType : ValueObject
{
    public static readonly CustomerType Individual = new("Individual");
    public static readonly CustomerType Corporate = new("Corporate");
    public static readonly CustomerType VIP = new("VIP");

    public string Value { get; }

    private CustomerType(string value)
    {
        Value = value;
    }

    public static CustomerType Create(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Customer type cannot be null or empty", nameof(type));

        return type.Trim() switch
        {
            "Individual" => Individual,
            "Corporate" => Corporate,
            "VIP" => VIP,
            _ => throw new ArgumentException($"Invalid customer type: {type}", nameof(type))
        };
    }

    public bool IsVIP => this == VIP;
    public bool IsCorporate => this == Corporate;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(CustomerType customerType) => customerType.Value;
}