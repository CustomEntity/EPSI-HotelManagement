using HotelManagement.Domain.Common;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Booking.ValueObjects;

public sealed class GuestCount : ValueObject
{
    public int Adults { get; }
    public int Children { get; }
    public int Total => Adults + Children;

    private GuestCount(int adults, int children)
    {
        Adults = adults;
        Children = children;
    }

    public static Result<GuestCount> Create(int adults, int children = 0)
    {
        if (adults < 1)
            return Result<GuestCount>.Failure("At least one adult is required");

        if (adults > 10)
            return Result<GuestCount>.Failure("Number of adults cannot exceed 10");

        if (children < 0)
            return Result<GuestCount>.Failure("Number of children cannot be negative");

        if (children > 10)
            return Result<GuestCount>.Failure("Number of children cannot exceed 10");

        var total = adults + children;
        if (total > 20)
            return Result<GuestCount>.Failure("Total guest count cannot exceed 20");

        return Result<GuestCount>.Success(new GuestCount(adults, children));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Adults;
        yield return Children;
    }

    public override string ToString() => 
        Children > 0 ? $"{Adults} adult(s), {Children} child(ren)" : $"{Adults} adult(s)";
}