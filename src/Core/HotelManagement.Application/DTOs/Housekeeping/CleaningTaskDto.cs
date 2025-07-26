namespace HotelManagement.Application.DTOs.Housekeeping;

public sealed class CleaningTaskDto
{
    public Guid CleaningTaskId { get; init; }
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTime TaskCreatedAt { get; init; }
    public DateTime? ScheduledFor { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? AssignedTo { get; init; }
    public string? Notes { get; init; }
    public bool HasDamageReport { get; init; }
    public string? DamageDescription { get; init; }
    public decimal? EstimatedRepairCost { get; init; }
    public bool IsOverdue { get; init; }
    public TimeSpan? Duration { get; init; }
    public string? CompletionStatus { get; init; }
}

public sealed class CleaningTaskSummaryDto
{
    public Guid CleaningTaskId { get; init; }
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public DateTime? ScheduledFor { get; init; }
    public string? AssignedTo { get; init; }
    public bool HasDamageReport { get; init; }
    public bool IsOverdue { get; init; }
}
