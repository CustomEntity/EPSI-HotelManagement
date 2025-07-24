using HotelManagement.Domain.Shared;
using MediatR;

namespace HotelManagement.Application.Booking.Queries.GetAvailableRooms;

public sealed class GetAvailableRoomsQuery : IRequest<Result<List<AvailableRoomDto>>>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int? MinOccupancy { get; init; }
    public int? MaxOccupancy { get; init; }
}

public sealed class AvailableRoomDto
{
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public string RoomTypeName { get; init; } = string.Empty;
    public decimal PricePerNight { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int MaxOccupancy { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Condition { get; init; } = string.Empty;
}