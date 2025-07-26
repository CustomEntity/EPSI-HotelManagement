using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Booking.Events;

public sealed record RoomCheckedInEvent(
    BookingId BookingId,
    RoomId RoomId,
    DateTime CheckInTime
) : IDomainEvent
{
    public DateTime OccurredOn => CheckInTime;
} 