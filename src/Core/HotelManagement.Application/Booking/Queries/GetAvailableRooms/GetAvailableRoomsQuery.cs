using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Queries.GetAvailableRooms;

public sealed class GetAvailableRoomsQuery : IRequest<Result<List<AvailableRoomDto>>>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public sealed class AvailableRoomDto
{
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = null!;
    public string RoomType { get; init; } = null!;
    public string RoomStatus { get; init; } = null!;
    public string? RoomCondition { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}