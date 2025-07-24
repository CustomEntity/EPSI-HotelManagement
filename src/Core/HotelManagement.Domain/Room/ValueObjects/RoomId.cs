using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Room.ValueObjects;

public class RoomId : ValueObject
{
    public Guid Value { get; }

    public RoomId(Guid value)
    {
        Value = value;
    }

    public static RoomId Create() => new(Guid.NewGuid());

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}