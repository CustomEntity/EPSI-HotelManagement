using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Housekeeping.Services;

public interface IHousekeepingNotificationService
{
    Task NotifyTaskCreatedAsync(CleaningTaskId taskId, RoomId roomId, CleaningPriority priority);
    Task NotifyTaskCompletedAsync(CleaningTaskId taskId, RoomId roomId, string completedBy);
    Task NotifyDamageReportedAsync(CleaningTaskId taskId, RoomId roomId, string damageDescription);
    Task NotifyTaskOverdueAsync(CleaningTaskId taskId, RoomId roomId);
    Task NotifyHighPriorityTaskAsync(CleaningTaskId taskId, RoomId roomId);
}