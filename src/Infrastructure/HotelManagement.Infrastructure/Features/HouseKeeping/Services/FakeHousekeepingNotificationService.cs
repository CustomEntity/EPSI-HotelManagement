using HotelManagement.Domain.Housekeeping.Services;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Infrastructure.Features.HouseKeeping.Services;

public class FakeHousekeepingNotificationService : IHousekeepingNotificationService
{
    public Task NotifyTaskCreatedAsync(CleaningTaskId taskId, RoomId roomId, CleaningPriority priority)
    {
        Console.WriteLine($"NOTIFICATION: Cleaning task {taskId} created for room {roomId} with priority {priority}");
        return Task.CompletedTask;
    }

    public Task NotifyTaskCompletedAsync(CleaningTaskId taskId, RoomId roomId, string completedBy)
    {
        Console.WriteLine($"NOTIFICATION: Cleaning task {taskId} completed for room {roomId} by {completedBy}");
        return Task.CompletedTask;
    }

    public Task NotifyDamageReportedAsync(CleaningTaskId taskId, RoomId roomId, string damageDescription)
    {
        Console.WriteLine($"NOTIFICATION: Damage reported for task {taskId} in room {roomId}: {damageDescription}");
        return Task.CompletedTask;
    }

    public Task NotifyTaskOverdueAsync(CleaningTaskId taskId, RoomId roomId)
    {
        Console.WriteLine($"NOTIFICATION: Cleaning task {taskId} is overdue for room {roomId}");
        return Task.CompletedTask;
    }

    public Task NotifyHighPriorityTaskAsync(CleaningTaskId taskId, RoomId roomId)
    {
        Console.WriteLine($"NOTIFICATION: High priority cleaning task {taskId} assigned to room {roomId}");
        return Task.CompletedTask;
    }
}