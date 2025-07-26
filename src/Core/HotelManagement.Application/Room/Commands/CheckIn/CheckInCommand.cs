using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Room.Commands.CheckIn;

public sealed record CheckInCommand(
    Guid RoomId,
    Guid BookingId,
    DateTime CheckInTime
) : IRequest<Result>;