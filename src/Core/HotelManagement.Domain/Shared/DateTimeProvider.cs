namespace HotelManagement.Domain.Shared;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    
    DateTime Today { get; }
    
    DateTime LocalNow { get; }
    
    DateTime LocalToday { get; }
    
    bool IsDateInPast(DateTime date);
    
    bool IsDateWithinNextHours(DateTime date, double hours);
    
    double GetHoursUntil(DateTime futureDate);
    
    int GetDaysBetween(DateTime startDate, DateTime endDate);
}