using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Payment.ValueObjects;

public sealed class PaymentId : ValueObject
{
    public Guid Value { get; }

    private PaymentId(Guid value)
    {
        Value = value;
    }

    public static PaymentId New() => new(Guid.NewGuid());

    public static PaymentId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Payment ID cannot be empty", nameof(value));

        return new PaymentId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(PaymentId paymentId) => paymentId.Value;
    public static explicit operator PaymentId(Guid value) => Create(value);
}