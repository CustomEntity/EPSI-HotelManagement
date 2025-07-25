using HotelManagement.Application.DTOs.Housekeeping;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Housekeeping.Queries.GetCleaningTasks;

public sealed class GetCleaningTasksQuery : IRequest<Result<List<CleaningTaskDto>>>
{
    public string? Status { get; init; }
    public string? Priority { get; init; }
    public Guid? RoomId { get; init; }
    public string? AssignedTo { get; init; }
    public DateTime? ScheduledDate { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool? HasDamageReport { get; init; }
    public bool? IsOverdue { get; init; }
    public bool IncludeCompleted { get; init; } = true;
    public bool IncludeCancelled { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "Priority";
    public bool SortDescending { get; init; } = true;
}