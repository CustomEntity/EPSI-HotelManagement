using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Room.Commands.CheckOut;

public sealed record CheckOutCommand(
    Guid RoomId,
    Guid BookingId,
    DateTime CheckOutTime
) : IRequest<Result>;
