using HotelManagement.Domain.Booking;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Queries.GetAvailableRooms;

public sealed class
    GetAvailableRoomsQueryHandler : IRequestHandler<GetAvailableRoomsQuery, Result<List<AvailableRoomDto>>>
{
    private readonly IRoomRepository _roomRepository;

    public GetAvailableRoomsQueryHandler(
        IRoomRepository roomRepository
    )
    {
        _roomRepository = roomRepository;
    }

    public async Task<Result<List<AvailableRoomDto>>> Handle(
        GetAvailableRoomsQuery request,
        CancellationToken cancellationToken)
    {
        var dateRangeResult = DateRange.Create(request.StartDate, request.EndDate);
        if (dateRangeResult.IsFailure)
            return Result<List<AvailableRoomDto>>.Failure(dateRangeResult.Error);

        var dateRange = dateRangeResult.Value;

        var availableRooms = await _roomRepository.GetAvailableRoomsInDateRangeAsync(
            dateRange,
            cancellationToken);

        if (availableRooms.Count == 0)
            return Result<List<AvailableRoomDto>>.Success(new List<AvailableRoomDto>());

        var result = availableRooms.Select(room => new AvailableRoomDto
        {
            RoomId = room.Id.Value,
            RoomNumber = room.Number.Value,
            RoomType = room.Type.Name,
            RoomStatus = room.Status.ToString(),
            RoomCondition = room.Condition.ToString(),
            CreatedAt = room.CreatedAt,
            ModifiedAt = room.ModifiedAt
        }).ToList();

        return Result<List<AvailableRoomDto>>.Success(result);
    }
}