using HotelManagement.Domain.Common;

namespace HotelManagement.Domain.Booking.Entities;

public class CancellationPolicy : Entity
{
    public int HoursBeforeCheckIn { get; private set; }
    public decimal RefundPercentage { get; private set; }
    public string Description { get; private set; }

    private CancellationPolicy() 
    {
        Description = string.Empty;
    }

    public CancellationPolicy(int hoursBeforeCheckIn, decimal refundPercentage, string description)
    {
        if (hoursBeforeCheckIn < 0)
            throw new ArgumentException("Hours before check-in cannot be negative", nameof(hoursBeforeCheckIn));

        if (refundPercentage < 0 || refundPercentage > 100)
            throw new ArgumentException("Refund percentage must be between 0 and 100", nameof(refundPercentage));

        HoursBeforeCheckIn = hoursBeforeCheckIn;
        RefundPercentage = refundPercentage;
        Description = description;
    }

    public static CancellationPolicy Standard() => 
        new(48, 0, "Standard policy: No refund for cancellations within 48 hours");

    public static CancellationPolicy Flexible() => 
        new(24, 50, "Flexible policy: 50% refund for cancellations within 24 hours");

    public static CancellationPolicy Strict() => 
        new(72, 0, "Strict policy: No refund for cancellations within 72 hours");

    public bool IsRefundable(DateTime checkInDate)
    {
        var hoursUntilCheckIn = (checkInDate - DateTime.UtcNow).TotalHours;
        return hoursUntilCheckIn >= HoursBeforeCheckIn;
    }

    public decimal CalculateRefundPercentage(DateTime checkInDate)
    {
        return IsRefundable(checkInDate) ? 100 : RefundPercentage;
    }
}