using HotelManagement.Domain.Common;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Room.ValueObjects;

public class Capacity : ValueObject
{
    public int Value { get; }

    private Capacity(int value)
    {
        Value = value;
    }

    public static Result<Capacity> Create(int value)
    {
        if (value < 1)
            return Result<Capacity>.Failure("Capacity must be at least 1");

        if (value > 10)
            return Result<Capacity>.Failure("Capacity cannot exceed 10");

        return Result<Capacity>.Success(new Capacity(value));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}