using HotelManagement.Domain.Common;
using HotelManagement.Domain.Booking.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Booking.Events;

public sealed class GuestCheckedOutEvent : DomainEventBase
{
    public BookingId BookingId { get; }
    public List<RoomId> RoomIds { get; }
    public DateTime CheckOutTime { get; }

    public GuestCheckedOutEvent(BookingId bookingId, List<RoomId> roomIds, DateTime checkOutTime)
    {
        BookingId = bookingId;
        RoomIds = roomIds;
        CheckOutTime = checkOutTime;
    }
}