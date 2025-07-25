using HotelManagement.Application.DTOs.Room;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Room.Queries.GetRoomDetails;

public sealed class GetRoomDetailsQueryHandler : IRequestHandler<GetRoomDetailsQuery, Result<RoomDetailsDto>>
{
    private readonly IRoomRepository _roomRepository;

    public GetRoomDetailsQueryHandler(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<Result<RoomDetailsDto>> Handle(
        GetRoomDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var roomId = new RoomId(request.RoomId);
        
        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        if (room == null)
            return Result<RoomDetailsDto>.Failure("Room not found");

        var roomDetailsDto = new RoomDetailsDto
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
            ModifiedAt = room.ModifiedAt,
            Features = GetRoomFeatures(room),
            MaintenanceNotes = GetMaintenanceNotes(room)
        };

        return Result<RoomDetailsDto>.Success(roomDetailsDto);
    }

    private List<string> GetRoomFeatures(Domain.Room.Aggregates.Room room)
    {
        return new List<string> { "WiFi", "Air Conditioning", "Mini Bar" };
    }

    private List<string> GetMaintenanceNotes(Domain.Room.Aggregates.Room room)
    {
        return new List<string>();
    }
}