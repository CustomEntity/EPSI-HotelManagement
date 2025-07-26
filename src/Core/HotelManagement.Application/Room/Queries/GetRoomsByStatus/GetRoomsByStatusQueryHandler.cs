using HotelManagement.Application.DTOs.Room;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Room.Queries.GetRoomsByStatus;

public sealed class GetRoomsByStatusQueryHandler : IRequestHandler<GetRoomsByStatusQuery, Result<List<RoomSummaryDto>>>
{
    private readonly IRoomRepository _roomRepository;

    public GetRoomsByStatusQueryHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Result<List<RoomSummaryDto>>> Handle(
        GetRoomsByStatusQuery request,
        CancellationToken cancellationToken)
    {
        var rooms = await _roomRepository.GetAllAsync(cancellationToken);

        if (!string.IsNullOrEmpty(request.Status))
        {
            var requestedStatus = GetRoomStatusFromString(request.Status);
            if (requestedStatus != null)
            {
                rooms = rooms.Where(r => r.Status == requestedStatus).ToList();
            }
        }

        var pagedRooms = rooms
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var roomSummaryDtos = pagedRooms.Select(room => new RoomSummaryDto
        {
            RoomId = room.Id.Value,
            RoomNumber = room.Number.Value,
            RoomType = room.Type.Name,
            Status = room.Status.ToString(),
            Condition = room.Condition.ToString(),
            BasePrice = room.Type.BasePrice.Amount,
            Currency = room.Type.BasePrice.Currency.Code,
            MaxOccupancy = room.Type.MaxOccupancy,
            CreatedAt = room.CreatedAt,
            ModifiedAt = room.ModifiedAt
        }).ToList();

        return Result<List<RoomSummaryDto>>.Success(roomSummaryDtos);
    }

    private static RoomStatus? GetRoomStatusFromString(string status)
    {
        return status.ToLowerInvariant() switch
        {
            "available" => RoomStatus.Available,
            "occupied" => RoomStatus.Occupied,
            "maintenance" => RoomStatus.Maintenance,
            "cleaning" => RoomStatus.Cleaning,
            _ => null
        };
    }
}