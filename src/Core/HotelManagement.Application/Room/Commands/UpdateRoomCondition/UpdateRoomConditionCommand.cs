using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Room.Commands.UpdateRoomCondition;

public sealed record UpdateRoomConditionCommand(
    Guid RoomId,
    string Condition,
    string? Notes = null,
    string? ReportedBy = null
) : IRequest<Result>;
