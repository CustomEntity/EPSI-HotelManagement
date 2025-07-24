using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Room.ValueObjects;

public class RoomStatus : ValueObject
{
    public static readonly RoomStatus Available = new("Available");
    public static readonly RoomStatus Occupied = new("Occupied");
    public static readonly RoomStatus Maintenance = new("Maintenance");
    public static readonly RoomStatus Cleaning = new("Cleaning");

    public string Value { get; }

    private RoomStatus(string value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}