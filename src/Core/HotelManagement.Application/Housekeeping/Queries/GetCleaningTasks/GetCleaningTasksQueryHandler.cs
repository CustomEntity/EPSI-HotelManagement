using HotelManagement.Domain.Housekeeping.Repositories;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using HotelManagement.Application.DTOs.Housekeeping;

namespace HotelManagement.Application.Housekeeping.Queries.GetCleaningTasks;

public sealed class GetCleaningTasksQueryHandler : IRequestHandler<GetCleaningTasksQuery, Result<List<CleaningTaskDto>>>
{
    private readonly ICleaningTaskRepository _cleaningTaskRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ILogger<GetCleaningTasksQueryHandler> _logger;

    public GetCleaningTasksQueryHandler(
        ICleaningTaskRepository cleaningTaskRepository,
        IRoomRepository roomRepository,
        ILogger<GetCleaningTasksQueryHandler> logger)
    {
        _cleaningTaskRepository = cleaningTaskRepository;
        _roomRepository = roomRepository;
        _logger = logger;
    }

    public async Task<Result<List<CleaningTaskDto>>> Handle(
        GetCleaningTasksQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Retrieving cleaning tasks with filters: Status={Status}, Priority={Priority}, " +
                "RoomId={RoomId}, AssignedTo={AssignedTo}, IsOverdue={IsOverdue}",
                request.Status, request.Priority, request.RoomId, 
                request.AssignedTo, request.IsOverdue);

            var cleaningTasks = await GetFilteredTasksAsync(request, cancellationToken);

            cleaningTasks = ApplyAdditionalFilters(cleaningTasks, request);

            cleaningTasks = ApplySorting(cleaningTasks, request.SortBy, request.SortDescending);

            var totalCount = cleaningTasks.Count;
            var pagedTasks = cleaningTasks
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var cleaningTaskDtos = new List<CleaningTaskDto>();

            foreach (var task in pagedTasks)
            {
                var room = await _roomRepository.GetByIdAsync(task.RoomId, cancellationToken);
                var roomNumber = room?.Number.Value ?? "Unknown";

                var dto = new CleaningTaskDto
                {
                    CleaningTaskId = task.Id.Value,
                    RoomId = task.RoomId.Value,
                    RoomNumber = roomNumber,
                    Status = task.Status.Value,
                    Priority = task.Priority.Name,
                    TaskCreatedAt = task.TaskCreatedAt,
                    ScheduledFor = task.ScheduledFor,
                    StartedAt = task.StartedAt,
                    CompletedAt = task.CompletedAt,
                    AssignedTo = task.AssignedTo,
                    Notes = task.Notes,
                    HasDamageReport = task.HasDamageReport,
                    DamageDescription = task.DamageDescription,
                    EstimatedRepairCost = task.EstimatedRepairCost,
                    IsOverdue = task.IsOverdue,
                    Duration = task.Duration,
                    CompletionStatus = GetCompletionStatus(task)
                };

                cleaningTaskDtos.Add(dto);
            }

            _logger.LogInformation(
                "Retrieved {TaskCount} cleaning tasks out of {TotalCount} total",
                cleaningTaskDtos.Count, totalCount);

            return Result<List<CleaningTaskDto>>.Success(cleaningTaskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving cleaning tasks");
            return Result<List<CleaningTaskDto>>.Failure("An error occurred while retrieving cleaning tasks");
        }
    }

    private async Task<List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate>> GetFilteredTasksAsync(
        GetCleaningTasksQuery request,
        CancellationToken cancellationToken)
    {
        if (request.RoomId.HasValue)
        {
            var roomId = new RoomId(request.RoomId.Value);
            var tasks = await _cleaningTaskRepository.GetTasksByRoomAsync(roomId);
            return tasks.ToList();
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            var status = CleaningStatus.FromString(request.Status);
            if (status != null)
            {
                var tasks = await _cleaningTaskRepository.GetTasksByStatusAsync(status);
                return tasks.ToList();
            }
        }

        if (!string.IsNullOrEmpty(request.Priority))
        {
            var priority = CleaningPriority.FromString(request.Priority);
            if (priority != null)
            {
                var tasks = await _cleaningTaskRepository.GetTasksByPriorityAsync(priority);
                return tasks.ToList();
            }
        }

        if (!string.IsNullOrEmpty(request.AssignedTo))
        {
            var tasks = await _cleaningTaskRepository.GetTasksAssignedToAsync(request.AssignedTo);
            return tasks.ToList();
        }

        if (request.IsOverdue == true)
        {
            var tasks = await _cleaningTaskRepository.GetOverdueTasksAsync();
            return tasks.ToList();
        }

        if (request.ScheduledDate.HasValue)
        {
            var tasks = await _cleaningTaskRepository.GetTasksForDateAsync(request.ScheduledDate.Value);
            return tasks.ToList();
        }

        var pendingTasks = await _cleaningTaskRepository.GetPendingTasksAsync();
        return pendingTasks.ToList();
    }

    private static List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate> ApplyAdditionalFilters(
        List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate> tasks,
        GetCleaningTasksQuery request)
    {
        var filteredTasks = tasks.AsQueryable();

        if (request.FromDate.HasValue)
        {
            filteredTasks = filteredTasks.Where(t => t.TaskCreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            filteredTasks = filteredTasks.Where(t => t.TaskCreatedAt <= request.ToDate.Value);
        }

        if (request.HasDamageReport.HasValue)
        {
            filteredTasks = filteredTasks.Where(t => t.HasDamageReport == request.HasDamageReport.Value);
        }

        if (!request.IncludeCompleted)
        {
            filteredTasks = filteredTasks.Where(t => t.Status != CleaningStatus.Completed);
        }

        if (!request.IncludeCancelled)
        {
            filteredTasks = filteredTasks.Where(t => t.Status != CleaningStatus.Cancelled);
        }

        return filteredTasks.ToList();
    }

    private static List<Domain.Housekeeping.Aggregates.CleaningTaskAggregate> ApplySorting(
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
            "status" => sortDescending
                ? query.OrderByDescending(t => t.Status.Value)
                : query.OrderBy(t => t.Status.Value),
            "scheduledfor" => sortDescending
                ? query.OrderByDescending(t => t.ScheduledFor)
                : query.OrderBy(t => t.ScheduledFor),
            "createdat" => sortDescending
                ? query.OrderByDescending(t => t.TaskCreatedAt)
                : query.OrderBy(t => t.TaskCreatedAt),
            "assignedto" => sortDescending
                ? query.OrderByDescending(t => t.AssignedTo)
                : query.OrderBy(t => t.AssignedTo),
            "roomid" => sortDescending
                ? query.OrderByDescending(t => t.RoomId.Value)
                : query.OrderBy(t => t.RoomId.Value),
            _ => sortDescending
                ? query.OrderByDescending(t => t.Priority.Level).ThenByDescending(t => t.TaskCreatedAt)
                : query.OrderBy(t => t.Priority.Level).ThenBy(t => t.TaskCreatedAt)
        };

        return query.ToList();
    }

    private static string GetCompletionStatus(Domain.Housekeeping.Aggregates.CleaningTaskAggregate task)
    {
        if (task.Status == CleaningStatus.Completed)
        {
            return task.HasDamageReport ? "Completed with damage" : "Completed successfully";
        }

        if (task.Status == CleaningStatus.Cancelled)
        {
            return "Cancelled";
        }

        if (task.IsOverdue)
        {
            return "Overdue";
        }

        if (task.Status == CleaningStatus.InProgress)
        {
            return "In progress";
        }

        return "Pending";
    }
}