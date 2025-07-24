using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.Aggregates;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Housekeeping.Repositories;

public interface ICleaningTaskRepository : IRepository<CleaningTaskAggregate, CleaningTaskId>
{
    Task<IEnumerable<CleaningTaskAggregate>> GetTasksByRoomAsync(RoomId roomId);
    Task<IEnumerable<CleaningTaskAggregate>> GetTasksByStatusAsync(CleaningStatus status);
    Task<IEnumerable<CleaningTaskAggregate>> GetTasksByPriorityAsync(CleaningPriority priority);
    Task<IEnumerable<CleaningTaskAggregate>> GetTasksAssignedToAsync(string assignee);
    Task<IEnumerable<CleaningTaskAggregate>> GetOverdueTasksAsync();
    Task<IEnumerable<CleaningTaskAggregate>> GetTasksForDateAsync(DateTime date);
    Task<IEnumerable<CleaningTaskAggregate>> GetPendingTasksAsync();
    Task<IEnumerable<CleaningTaskAggregate>> GetTasksWithDamageReportsAsync();
    Task<bool> HasPendingTaskForRoomAsync(RoomId roomId);
    Task<CleaningTaskAggregate?> GetActiveTaskForRoomAsync(RoomId roomId);
}