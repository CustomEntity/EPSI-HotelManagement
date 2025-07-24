using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Booking.ValueObjects;

public sealed class BookingId : ValueObject
{
    public Guid Value { get; }

    private BookingId(Guid value)
    {
        Value = value;
    }

    public static BookingId New() => new(Guid.NewGuid());

    public static BookingId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Booking ID cannot be empty", nameof(value));

        return new BookingId(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(BookingId bookingId) => bookingId.Value;
    public static explicit operator BookingId(Guid value) => Create(value);
}