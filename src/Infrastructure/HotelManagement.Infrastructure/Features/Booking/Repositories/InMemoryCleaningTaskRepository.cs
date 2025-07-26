using HotelManagement.Domain.Housekeeping.Aggregates;
using HotelManagement.Domain.Housekeeping.Repositories;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Infrastructure.Features.Repositories;

public class InMemoryCleaningTaskRepository : ICleaningTaskRepository
{
    private readonly List<CleaningTaskAggregate> _cleaningTasks = new();

    public InMemoryCleaningTaskRepository()
    {
        SeedTestData();
    }

    public Task<CleaningTaskAggregate?> GetByIdAsync(CleaningTaskId id, CancellationToken cancellationToken = default)
    {
        var task = _cleaningTasks.FirstOrDefault(t => t.Id.Value == id.Value);
        return Task.FromResult(task);
    }

    public Task<List<CleaningTaskAggregate>> GetByRoomIdAsync(RoomId roomId, CancellationToken cancellationToken = default)
    {
        var tasks = _cleaningTasks
            .Where(t => t.RoomId.Value == roomId.Value)
            .ToList();
        return Task.FromResult(tasks);
    }

    public Task<List<CleaningTaskAggregate>> GetByStatusAsync(CleaningStatus status, CancellationToken cancellationToken = default)
    {
        var tasks = _cleaningTasks
            .Where(t => t.Status == status)
            .ToList();
        return Task.FromResult(tasks);
    }

    public Task<List<CleaningTaskAggregate>> GetByPriorityAsync(CleaningPriority priority, CancellationToken cancellationToken = default)
    {
        var tasks = _cleaningTasks
            .Where(t => t.Priority == priority)
            .ToList();
        return Task.FromResult(tasks);
    }

    public Task<List<CleaningTaskAggregate>> GetPendingTasksAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _cleaningTasks
            .Where(t => t.Status == CleaningStatus.Pending || t.Status == CleaningStatus.InProgress)
            .OrderBy(t => t.Priority.Level)
            .ThenBy(t => t.ScheduledFor ?? t.TaskCreatedAt)
            .ToList();
        return Task.FromResult(tasks);
    }

    public Task<List<CleaningTaskAggregate>> GetTasksByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        var tasks = _cleaningTasks
            .Where(t => (t.ScheduledFor ?? t.TaskCreatedAt).Date >= fromDate.Date && 
                       (t.ScheduledFor ?? t.TaskCreatedAt).Date <= toDate.Date)
            .ToList();
        return Task.FromResult(tasks);
    }

    public Task<List<CleaningTaskAggregate>> GetOverdueTasksAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var tasks = _cleaningTasks
            .Where(t => !t.Status.IsFinal && 
                       t.ScheduledFor.HasValue && 
                       t.ScheduledFor.Value < now)
            .ToList();
        return Task.FromResult(tasks);
    }

    public Task<List<CleaningTaskAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cleaningTasks.ToList());
    }

    // Nouvelles méthodes de l'interface

    public Task<CleaningTaskAggregate?> GetActiveTaskForRoomAsync(RoomId roomId)
    {
        var task = _cleaningTasks
            .FirstOrDefault(t => t.RoomId.Value == roomId.Value && 
                               (t.Status == CleaningStatus.Pending || t.Status == CleaningStatus.InProgress));
        return Task.FromResult(task);
    }

    public Task<IEnumerable<CleaningTaskAggregate>> GetOverdueTasksAsync()
    {
        var now = DateTime.UtcNow;
        var tasks = _cleaningTasks
            .Where(t => !t.Status.IsFinal && 
                       t.ScheduledFor.HasValue && 
                       t.ScheduledFor.Value < now)
            .AsEnumerable();
        return Task.FromResult(tasks);
    }

    public Task<IEnumerable<CleaningTaskAggregate>> GetPendingTasksAsync()
    {
        var tasks = _cleaningTasks
            .Where(t => t.Status == CleaningStatus.Pending || t.Status == CleaningStatus.InProgress)
            .OrderBy(t => t.Priority.Level)
            .ThenBy(t => t.ScheduledFor ?? t.TaskCreatedAt)
            .AsEnumerable();
        return Task.FromResult(tasks);
    }

    public Task<IEnumerable<CleaningTaskAggregate>> GetTasksAssignedToAsync(string assignedTo)
    {
        var tasks = _cleaningTasks
            .Where(t => !string.IsNullOrEmpty(t.AssignedTo) && 
                       t.AssignedTo.Equals(assignedTo, StringComparison.OrdinalIgnoreCase))
            .AsEnumerable();
        return Task.FromResult(tasks);
    }

    public Task<IEnumerable<CleaningTaskAggregate>> GetTasksByPriorityAsync(CleaningPriority priority)
    {
        var tasks = _cleaningTasks
            .Where(t => t.Priority == priority)
            .AsEnumerable();
        return Task.FromResult(tasks);
    }

    public Task<IEnumerable<CleaningTaskAggregate>> GetTasksByRoomAsync(RoomId roomId)
    {
        var tasks = _cleaningTasks
            .Where(t => t.RoomId.Value == roomId.Value)
            .AsEnumerable();
        return Task.FromResult(tasks);
    }

    public Task<IEnumerable<CleaningTaskAggregate>> GetTasksByStatusAsync(CleaningStatus status)
    {
        var tasks = _cleaningTasks
            .Where(t => t.Status == status)
            .AsEnumerable();
        return Task.FromResult(tasks);
    }

    public Task<IEnumerable<CleaningTaskAggregate>> GetTasksForDateAsync(DateTime date)
    {
        var tasks = _cleaningTasks
            .Where(t => (t.ScheduledFor ?? t.TaskCreatedAt).Date == date.Date)
            .AsEnumerable();
        return Task.FromResult(tasks);
    }

    public Task<IEnumerable<CleaningTaskAggregate>> GetTasksWithDamageReportsAsync()
    {
        var tasks = _cleaningTasks
            .Where(t => t.HasDamageReport)
            .AsEnumerable();
        return Task.FromResult(tasks);
    }

    public Task<bool> HasPendingTaskForRoomAsync(RoomId roomId)
    {
        var hasPendingTask = _cleaningTasks
            .Any(t => t.RoomId.Value == roomId.Value && 
                     (t.Status == CleaningStatus.Pending || t.Status == CleaningStatus.InProgress));
        return Task.FromResult(hasPendingTask);
    }

    // Méthodes CRUD

    public Task AddAsync(CleaningTaskAggregate entity, CancellationToken cancellationToken = default)
    {
        _cleaningTasks.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CleaningTaskAggregate entity, CancellationToken cancellationToken = default)
    {
        var existingTask = _cleaningTasks.FirstOrDefault(t => t.Id.Value == entity.Id.Value);
        if (existingTask != null)
        {
            _cleaningTasks.Remove(existingTask);
            _cleaningTasks.Add(entity);
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CleaningTaskAggregate entity, CancellationToken cancellationToken = default)
    {
        var task = _cleaningTasks.FirstOrDefault(t => t.Id.Value == entity.Id.Value);
        if (task != null)
        {
            _cleaningTasks.Remove(task);
        }
        return Task.CompletedTask;
    }

    private void SeedTestData()
    {
        try
        {
            var task1 = CreateTestCleaningTask(
                CleaningPriority.Normal,
                DateTime.UtcNow.AddHours(2),
                "Standard room cleaning");

            var task2 = CreateTestCleaningTask(
                CleaningPriority.High,
                DateTime.UtcNow.AddHours(1),
                "Deep cleaning after checkout");

            var task3 = CreateTestCleaningTask(
                CleaningPriority.Low,
                DateTime.UtcNow.AddDays(1),
                "Maintenance check");

            var task4 = CreateTestCleaningTaskWithDamage(
                CleaningPriority.Urgent,
                DateTime.UtcNow.AddMinutes(-30), // Tâche en retard
                "Emergency cleaning - damage reported");

            if (task1 != null) _cleaningTasks.Add(task1);
            if (task2 != null) _cleaningTasks.Add(task2);
            if (task3 != null) _cleaningTasks.Add(task3);
            if (task4 != null) 
            {
                _cleaningTasks.Add(task4);
                // Simuler un rapport de dégâts
                task4.ReportDamage("Broken window", 150.00m);
            }
        }
        catch
        {
            // Ignore seeding errors
        }
    }

    private CleaningTaskAggregate? CreateTestCleaningTask(
        CleaningPriority priority,
        DateTime scheduledFor,
        string notes)
    {
        try
        {
            var roomId = RoomId.Create(); // Utilise la méthode Create() sans paramètre

            var taskResult = CleaningTaskAggregate.Create(
                roomId,
                priority,
                scheduledFor,
                notes);

            return taskResult.IsSuccess ? taskResult.Value : null;
        }
        catch
        {
            return null;
        }
    }

    private CleaningTaskAggregate? CreateTestCleaningTaskWithDamage(
        CleaningPriority priority,
        DateTime scheduledFor,
        string notes)
    {
        try
        {
            var roomId = RoomId.Create(); // Utilise la méthode Create() sans paramètre

            var taskResult = CleaningTaskAggregate.Create(
                roomId,
                priority,
                scheduledFor,
                notes);

            if (taskResult.IsSuccess)
            {
                var task = taskResult.Value;
                // Simuler le démarrage de la tâche
                task.Start("John Doe");
                return task;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}