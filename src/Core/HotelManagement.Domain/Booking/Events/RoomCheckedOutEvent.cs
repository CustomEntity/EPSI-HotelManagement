using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Booking.Events;

public sealed record RoomCheckedOutEvent(
    BookingId BookingId,
    RoomId RoomId,
    DateTime CheckOutTime
) : IDomainEvent
{
    public DateTime OccurredOn => CheckOutTime;
} 