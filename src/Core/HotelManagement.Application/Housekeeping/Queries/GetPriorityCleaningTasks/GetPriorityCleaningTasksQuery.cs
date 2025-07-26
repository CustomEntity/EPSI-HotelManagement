using HotelManagement.Application.DTOs.Housekeeping;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Housekeeping.Queries.GetPriorityCleaningTasks;

public sealed class GetPriorityCleaningTasksQuery : IRequest<Result<List<CleaningTaskSummaryDto>>>
{
    public string MinimumPriority { get; init; } = "High";
    public string? Status { get; init; }
    public Guid? RoomId { get; init; }
    public string? AssignedTo { get; init; }
    public bool IncludeCompleted { get; init; } = false;
    public bool IncludeCancelled { get; init; } = false;
    public bool OnlyOverdue { get; init; } = false;
    public bool OnlyUnassigned { get; init; } = false;
    public double? DueWithinHours { get; init; }
    public int MaxResults { get; init; } = 50;
    public string SortBy { get; init; } = "Priority";
    public bool SortDescending { get; init; } = true;
}