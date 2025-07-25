using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Booking.ValueObjects;

public sealed class BookingItemStatus : ValueObject
{
    public static readonly BookingItemStatus Confirmed = new("Confirmed");
    public static readonly BookingItemStatus CheckedIn = new("CheckedIn");
    public static readonly BookingItemStatus CheckedOut = new("CheckedOut");

    public string Value { get; }

    private BookingItemStatus(string value)
    {
        Value = value;
    }

    public bool CanCheckIn() => this == Confirmed;
    public bool CanCheckOut() => this == CheckedIn;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
    public static implicit operator string(BookingItemStatus status) => status.Value;
} 