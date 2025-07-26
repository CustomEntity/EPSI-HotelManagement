using HotelManagement.Application.DTOs.Room;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Room.Queries.GetRoomDetails;

public sealed record GetRoomDetailsQuery(
    Guid RoomId
) : IRequest<Result<RoomDetailsDto>>;