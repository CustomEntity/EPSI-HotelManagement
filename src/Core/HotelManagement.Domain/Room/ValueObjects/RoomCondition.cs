using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Room.ValueObjects;

public class RoomCondition : ValueObject
{
    public static readonly RoomCondition New = new("New");
    public static readonly RoomCondition Refurbished = new("Refurbished");
    public static readonly RoomCondition NeedsRefurbishment = new("NeedsRefurbishment");
    public static readonly RoomCondition Good = new("Good");
    public static readonly RoomCondition Damaged = new("Damaged");

    public string Value { get; }

    private RoomCondition(string value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}