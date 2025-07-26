using HotelManagement.Application.DTOs.Room;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Room.Queries.GetRoomsByStatus;

public sealed record GetRoomsByStatusQuery(
    string? Status = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<List<RoomSummaryDto>>>;