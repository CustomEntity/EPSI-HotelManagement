using HotelManagement.Domain.Housekeeping.Aggregates;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Housekeeping.Services;

public interface ICleaningScheduleService
{
    Task<Result<CleaningTaskAggregate>> ScheduleCleaningAsync(
        RoomId roomId, 
        CleaningPriority priority, 
        DateTime? preferredTime = null);
    
    Task<Result> RescheduleTaskAsync(CleaningTaskId taskId, DateTime newTime);
    Task<IEnumerable<CleaningTaskAggregate>> GetOptimalCleaningScheduleAsync(DateTime date);
    Task<Result> AutoAssignTasksAsync(IEnumerable<string> availableStaff);
}