using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.Events;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Housekeeping.Aggregates;

public class CleaningTaskAggregate : AggregateRoot<CleaningTaskId>
{
    public CleaningTaskId Id { get; private set; } = null!;
    public RoomId RoomId { get; private set; } = null!;
    public CleaningStatus Status { get; private set; } = null!;
    public CleaningPriority Priority { get; private set; } = null!;
    public DateTime TaskCreatedAt { get; private set; }
    public DateTime? ScheduledFor { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? AssignedTo { get; private set; }
    public string? Notes { get; private set; }
    public bool HasDamageReport { get; private set; }
    public string? DamageDescription { get; private set; }
    public decimal? EstimatedRepairCost { get; private set; }

    private CleaningTaskAggregate() { } // EF Constructor

    private CleaningTaskAggregate(
        CleaningTaskId id,
        RoomId roomId,
        CleaningPriority priority,
        DateTime? scheduledFor = null,
        string? notes = null)
    {
        Id = id;
        RoomId = roomId;
        Status = CleaningStatus.Pending;
        Priority = priority;
        TaskCreatedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        ScheduledFor = scheduledFor;
        Notes = notes;
        HasDamageReport = false;
    }

    public static Result<CleaningTaskAggregate> Create(
        RoomId roomId,
        CleaningPriority priority,
        DateTime? scheduledFor = null,
        string? notes = null)
    {
        if (roomId is null)
            return Result<CleaningTaskAggregate>.Failure("RoomId cannot be null");

        if (priority is null)
            return Result<CleaningTaskAggregate>.Failure("Priority cannot be null");

        if (scheduledFor.HasValue && scheduledFor.Value < DateTime.UtcNow)
            return Result<CleaningTaskAggregate>.Failure("Scheduled time cannot be in the past");

        var cleaningTaskId = CleaningTaskId.CreateUnique();
        var cleaningTask = new CleaningTaskAggregate(
            cleaningTaskId,
            roomId,
            priority,
            scheduledFor,
            notes);

        cleaningTask.AddDomainEvent(new CleaningTaskCreatedEvent(
            cleaningTask.Id,
            cleaningTask.RoomId,
            cleaningTask.Priority,
            cleaningTask.TaskCreatedAt));

        return Result<CleaningTaskAggregate>.Success(cleaningTask);
    }

    public Result Start(string assignedTo)
    {
        if (string.IsNullOrWhiteSpace(assignedTo))
            return Result.Failure("AssignedTo cannot be empty");

        if (Status != CleaningStatus.Pending)
            return Result.Failure($"Cannot start cleaning task with status {Status}");

        Status = CleaningStatus.InProgress;
        AssignedTo = assignedTo;
        StartedAt = DateTime.UtcNow;

        AddDomainEvent(new CleaningTaskStartedEvent(
            Id,
            RoomId,
            assignedTo,
            StartedAt.Value));

        return Result.Success();
    }

    public Result Complete(string? completionNotes = null)
    {
        if (Status != CleaningStatus.InProgress)
            return Result.Failure($"Cannot complete cleaning task with status {Status}");

        Status = CleaningStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        
        if (!string.IsNullOrWhiteSpace(completionNotes))
        {
            Notes = string.IsNullOrEmpty(Notes) 
                ? completionNotes 
                : $"{Notes}\n--- Completion Notes ---\n{completionNotes}";
        }

        AddDomainEvent(new CleaningCompletedEvent(
            Id,
            RoomId,
            CompletedAt.Value,
            AssignedTo!));

        return Result.Success();
    }

    public Result ReportDamage(string damageDescription, decimal? estimatedRepairCost = null)
    {
        if (string.IsNullOrWhiteSpace(damageDescription))
            return Result.Failure("Damage description cannot be empty");

        if (estimatedRepairCost.HasValue && estimatedRepairCost.Value < 0)
            return Result.Failure("Estimated repair cost cannot be negative");

        HasDamageReport = true;
        DamageDescription = damageDescription;
        EstimatedRepairCost = estimatedRepairCost;

        AddDomainEvent(new DamageReportedEvent(
            Id,
            RoomId,
            damageDescription,
            estimatedRepairCost,
            DateTime.UtcNow));

        return Result.Success();
    }

    public Result Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("Cancellation reason cannot be empty");

        if (Status == CleaningStatus.Completed)
            return Result.Failure("Cannot cancel completed cleaning task");

        Status = CleaningStatus.Cancelled;
        Notes = string.IsNullOrEmpty(Notes) 
            ? $"Cancelled: {reason}" 
            : $"{Notes}\n--- Cancelled ---\n{reason}";

        AddDomainEvent(new CleaningTaskCancelledEvent(
            Id,
            RoomId,
            reason,
            DateTime.UtcNow));

        return Result.Success();
    }

    public Result Reschedule(DateTime newScheduledTime)
    {
        if (newScheduledTime < DateTime.UtcNow)
            return Result.Failure("Cannot reschedule to past time");

        if (Status == CleaningStatus.Completed || Status == CleaningStatus.Cancelled)
            return Result.Failure($"Cannot reschedule task with status {Status}");

        ScheduledFor = newScheduledTime;

        return Result.Success();
    }

    public Result UpdatePriority(CleaningPriority newPriority)
    {
        if (newPriority is null)
            return Result.Failure("Priority cannot be null");

        if (Status == CleaningStatus.Completed || Status == CleaningStatus.Cancelled)
            return Result.Failure($"Cannot update priority for task with status {Status}");

        Priority = newPriority;

        return Result.Success();
    }

    public bool IsOverdue => ScheduledFor.HasValue && 
                            ScheduledFor.Value < DateTime.UtcNow && 
                            Status != CleaningStatus.Completed && 
                            Status != CleaningStatus.Cancelled;

    public TimeSpan? Duration => StartedAt.HasValue && CompletedAt.HasValue 
        ? CompletedAt.Value - StartedAt.Value 
        : null;
}