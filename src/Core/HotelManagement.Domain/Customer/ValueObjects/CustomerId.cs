using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Customer.ValueObjects;

public sealed class CustomerId : ValueObject
{
    public Guid Value { get; }

    private CustomerId(Guid value)
    {
        Value = value;
    }

    public static CustomerId New() => new(Guid.NewGuid());
    
    public static CustomerId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(value));
        
        return new CustomerId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CustomerId customerId) => customerId.Value;
    public static explicit operator CustomerId(Guid value) => Create(value);
}