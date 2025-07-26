using HotelManagement.Domain.Shared;
using MediatR;
using HotelManagement.Application.DTOs.Booking;

namespace HotelManagement.Application.Booking.Queries.GetAvailableRooms;

public sealed class GetAvailableRoomsQuery : IRequest<Result<List<AvailableRoomDto>>>
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}