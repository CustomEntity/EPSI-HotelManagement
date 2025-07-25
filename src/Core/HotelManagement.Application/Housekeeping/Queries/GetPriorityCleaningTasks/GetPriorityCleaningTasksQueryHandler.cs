using HotelManagement.Application.DTOs.Housekeeping;
using HotelManagement.Domain.Housekeeping.Repositories;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Housekeeping.Queries.GetPriorityCleaningTasks;

public sealed class GetPriorityCleaningTasksQueryHandler : IRequestHandler<GetPriorityCleaningTasksQuery, Result<List<CleaningTaskSummaryDto>>>
{
    private readonly ICleaningTaskRepository _cleaningTaskRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<GetPriorityCleaningTasksQueryHandler> _logger;

    public GetPriorityCleaningTasksQueryHandler(
        ICleaningTaskRepository cleaningTaskRepository,
        IRoomRepository roomRepository,
        ILogger<GetPriorityCleaningTasksQueryHandler> logger)
    {
        _cleaningTaskRepository = cleaningTaskRepository;
        _roomRepository = roomRepository;
        _logger = logger;
    }

    public async Task<Result<List<CleaningTaskSummaryDto>>> Handle(
        GetPriorityCleaningTasksQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Retrieving priority cleaning tasks with MinimumPriority={MinimumPriority}, " +
                "OnlyOverdue={OnlyOverdue}, OnlyUnassigned={OnlyUnassigned}",
                request.MinimumPriority, request.OnlyOverdue, request.OnlyUnassigned);

            var minimumPriorityResult = GetMinimumPriority(request.MinimumPriority);
            if (minimumPriorityResult.IsFailure)
            {
                return Result<List<CleaningTaskSummaryDto>>.Failure(minimumPriorityResult.Error);
            }

            var minimumPriority = minimumPriorityResult.Value;

            var cleaningTasks = await GetPriorityTasksAsync(request, minimumPriority, cancellationToken);

            cleaningTasks = ApplyPriorityFilters(cleaningTasks, request, minimumPriority);

            cleaningTasks = ApplyPrioritySorting(cleaningTasks, request.SortBy, request.SortDescending);

            cleaningTasks = cleaningTasks.Take(request.MaxResults).ToList();

            var priorityTaskDtos = new List<CleaningTaskSummaryDto>();

            foreach (var task in cleaningTasks)
            {
                var room = await _roomRepository.GetByIdAsync(task.RoomId, cancellationToken);
                var roomNumber = room?.Number.Value ?? "Unknown";

                var dto = new CleaningTaskSummaryDto
                {
                    CleaningTaskId = task.Id.Value,
                    RoomId = task.RoomId.Value,
                    RoomNumber = roomNumber,
                    Status = task.Status.Value,
                    Priority = task.Priority.Name,
                    ScheduledFor = task.ScheduledFor,
                    AssignedTo = task.AssignedTo,
                    HasDamageReport = task.HasDamageReport,
                    IsOverdue = task.IsOverdue
                };

                priorityTaskDtos.Add(dto);
            }

            _logger.LogInformation(
                "Retrieved {TaskCount} priority cleaning tasks (minimum priority: {MinimumPriority})",
                priorityTaskDtos.Count, request.MinimumPriority);

            return Result<List<CleaningTaskSummaryDto>>.Success(priorityTaskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving priority cleaning tasks");
            return Result<List<CleaningTaskSummaryDto>>.Failure("An error occurred while retrieving priority cleaning tasks");
        }
    }

    private async Task<List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate>> GetPriorityTasksAsync(
        GetPriorityCleaningTasksQuery request,
        CleaningPriority minimumPriority,
        CancellationToken cancellationToken)
    {
        var allTasks = new List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate>();

        if (request.OnlyOverdue)
        {
            var overdueTasks = await _cleaningTaskRepository.GetOverdueTasksAsync();
            allTasks.AddRange(overdueTasks);
        }
        else
        { 
            var prioritiesToInclude = GetPrioritiesToInclude(minimumPriority);
            
            foreach (var priority in prioritiesToInclude)
            {
                var tasks = await _cleaningTaskRepository.GetTasksByPriorityAsync(priority);
                allTasks.AddRange(tasks);
            }
        }

        if (request.RoomId.HasValue)
        {
            var roomId = new RoomId(request.RoomId.Value);
            var roomTasks = await _cleaningTaskRepository.GetTasksByRoomAsync(roomId);
            
            allTasks = allTasks.Where(t => roomTasks.Any(rt => rt.Id.Value == t.Id.Value)).ToList();
        }

        if (!string.IsNullOrEmpty(request.AssignedTo))
        {
            var assignedTasks = await _cleaningTaskRepository.GetTasksAssignedToAsync(request.AssignedTo);
            allTasks = allTasks.Where(t => assignedTasks.Any(at => at.Id.Value == t.Id.Value)).ToList();
        }

        return allTasks.GroupBy(t => t.Id.Value).Select(g => g.First()).ToList();
    }

    private static Result<CleaningPriority> GetMinimumPriority(string minimumPriorityString)
    {
        var priority = CleaningPriority.FromString(minimumPriorityString);
        if (priority == null)
        {
            return Result<CleaningPriority>.Failure($"Invalid minimum priority: {minimumPriorityString}");
        }

        return Result<CleaningPriority>.Success(priority);
    }

    private static List<CleaningPriority> GetPrioritiesToInclude(CleaningPriority minimumPriority)
    {
        var priorities = new List<CleaningPriority>();
        
        if (minimumPriority.Level <= CleaningPriority.Low.Level)
            priorities.Add(CleaningPriority.Low);
        
        if (minimumPriority.Level <= CleaningPriority.Normal.Level)
            priorities.Add(CleaningPriority.Normal);
        
        if (minimumPriority.Level <= CleaningPriority.High.Level)
            priorities.Add(CleaningPriority.High);
        
        if (minimumPriority.Level <= CleaningPriority.Urgent.Level)
            priorities.Add(CleaningPriority.Urgent);

        return priorities;
    }

    private static List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate> ApplyPriorityFilters(
        List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate> tasks,
        GetPriorityCleaningTasksQuery request,
        CleaningPriority minimumPriority)
    {
        var filteredTasks = tasks.AsQueryable();

        filteredTasks = filteredTasks.Where(t => t.Priority.Level >= minimumPriority.Level);

        if (!string.IsNullOrEmpty(request.Status))
        {
            var status = CleaningStatus.FromString(request.Status);
            if (status != null)
            {
                filteredTasks = filteredTasks.Where(t => t.Status == status);
            }
        }

        if (!request.IncludeCompleted)
        {
            filteredTasks = filteredTasks.Where(t => t.Status != CleaningStatus.Completed);
        }

        if (!request.IncludeCancelled)
        {
            filteredTasks = filteredTasks.Where(t => t.Status != CleaningStatus.Cancelled);
        }

        if (request.OnlyUnassigned)
        {
            filteredTasks = filteredTasks.Where(t => string.IsNullOrEmpty(t.AssignedTo));
        }

        if (request.DueWithinHours.HasValue)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(request.DueWithinHours.Value);
            filteredTasks = filteredTasks.Where(t => 
                t.ScheduledFor.HasValue && t.ScheduledFor.Value <= cutoffTime);
        }

        return filteredTasks.ToList();
    }

    private static List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate> ApplyPrioritySorting(
        List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate> tasks,
        string sortBy,
        bool sortDescending)
    {
        var query = tasks.AsQueryable();

        query = sortBy.ToLowerInvariant() switch
        {
            "priority" => sortDescending
                ? query.OrderByDescending(t => t.Priority.Level)
                : query.OrderBy(t => t.Priority.Level),
            "scheduledfor" => query
                .OrderByDescending(t => t.Priority.Level)
                .ThenBy(t => t.ScheduledFor ?? DateTime.MaxValue),
            "status" => query
                .OrderByDescending(t => t.Priority.Level)
                .ThenBy(t => t.Status.Value),
            "overdue" => query
                .OrderByDescending(t => t.Priority.Level)
                .ThenByDescending(t => t.IsOverdue)
                .ThenBy(t => t.ScheduledFor ?? DateTime.MaxValue),
            _ => query
                .OrderByDescending(t => t.Priority.Level)
                .ThenByDescending(t => t.IsOverdue)
                .ThenBy(t => t.ScheduledFor ?? DateTime.MaxValue)
        };

        return query.ToList();
    }
}