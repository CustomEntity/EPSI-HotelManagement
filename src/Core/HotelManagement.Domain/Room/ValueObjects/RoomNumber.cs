using HotelManagement.Domain.Common;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Room.ValueObjects;

public class RoomNumber : ValueObject
{
    public string Value { get; }

    private RoomNumber(string value)
    {
        Value = value;
    }

    public static Result<RoomNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<RoomNumber>.Failure("Room number cannot be empty");

        if (value.Length > 10)
            return Result<RoomNumber>.Failure("Room number cannot exceed 10 characters");

        return Result<RoomNumber>.Success(new RoomNumber(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}