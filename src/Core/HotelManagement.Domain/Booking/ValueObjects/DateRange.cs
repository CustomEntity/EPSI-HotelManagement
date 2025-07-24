using HotelManagement.Domain.Common;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Booking.ValueObjects;

public sealed class DateRange : ValueObject
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate.Date; // Normaliser à minuit
        EndDate = endDate.Date;
    }

    public static Result<DateRange> Create(DateTime startDate, DateTime endDate)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        if (startDate >= endDate)
            return Result<DateRange>.Failure("Start date must be before end date");

        if (startDate < DateTime.UtcNow.Date)
            return Result<DateRange>.Failure("Start date cannot be in the past");

        if ((endDate - startDate).TotalDays > 365)
            return Result<DateRange>.Failure("Booking duration cannot exceed 365 days");

        return Result<DateRange>.Success(new DateRange(startDate, endDate));
    }

    public int GetNights() => (EndDate - StartDate).Days;

    public bool OverlapsWith(DateRange other)
    {
        return StartDate < other.EndDate && EndDate > other.StartDate;
    }

    public bool Contains(DateTime date)
    {
        var normalizedDate = date.Date;
        return normalizedDate >= StartDate && normalizedDate < EndDate;
    }

    public bool IsWithinNext48Hours()
    {
        var hoursUntilStart = (StartDate - DateTime.UtcNow).TotalHours;
        return hoursUntilStart <= 48 && hoursUntilStart >= 0;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd} - {EndDate:yyyy-MM-dd}";
}