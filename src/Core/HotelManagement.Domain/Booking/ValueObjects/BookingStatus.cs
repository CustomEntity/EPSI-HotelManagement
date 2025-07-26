using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Booking.ValueObjects;

public sealed class BookingStatus : ValueObject
{
    public static readonly BookingStatus Pending = new("Pending");
    public static readonly BookingStatus Confirmed = new("Confirmed");
    public static readonly BookingStatus PartiallyCheckedIn = new("PartiallyCheckedIn");
    public static readonly BookingStatus CheckedIn = new("CheckedIn");
    public static readonly BookingStatus CheckedOut = new("CheckedOut");
    public static readonly BookingStatus Cancelled = new("Cancelled");
    public static readonly BookingStatus NoShow = new("NoShow");

    public string Value { get; }

    private BookingStatus(string value)
    {
        Value = value;
    }

    public bool CanBeCancelled() => 
        this == Pending || this == Confirmed;

    public bool CanCheckIn() => 
        this == Confirmed || this == PartiallyCheckedIn;

    public bool CanCheckOut() => 
        this == CheckedIn || this == PartiallyCheckedIn;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(BookingStatus status) => status.Value;
    
    public static BookingStatus? FromString(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "pending" => Pending,
            "confirmed" => Confirmed,
            "partiallycheckedin" => PartiallyCheckedIn,
            "checkedin" => CheckedIn,
            "checkedout" => CheckedOut,
            "cancelled" => Cancelled,
            "noshow" => NoShow,
            _ => null
        };
    }
}