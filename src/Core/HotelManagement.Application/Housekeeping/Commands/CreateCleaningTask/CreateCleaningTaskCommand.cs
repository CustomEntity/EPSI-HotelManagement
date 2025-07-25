using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Housekeeping.Commands.CreateCleaningTask;

public sealed class CreateCleaningTaskCommand : IRequest<Result<Guid>>
{
    public Guid RoomId { get; init; }
    public string Priority { get; init; } = string.Empty;
    public DateTime? ScheduledFor { get; init; }
    public string? Notes { get; init; }
    public string? AssignedTo { get; init; }
}