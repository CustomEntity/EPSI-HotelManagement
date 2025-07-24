using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Booking.Events;

public sealed class GuestCheckedInEvent : DomainEventBase
{
    public BookingId BookingId { get; }
    public List<RoomId> RoomIds { get; }
    public DateTime CheckInTime { get; }

    public GuestCheckedInEvent(BookingId bookingId, List<RoomId> roomIds, DateTime checkInTime)
    {
        BookingId = bookingId;
        RoomIds = roomIds;
        CheckInTime = checkInTime;
    }
}