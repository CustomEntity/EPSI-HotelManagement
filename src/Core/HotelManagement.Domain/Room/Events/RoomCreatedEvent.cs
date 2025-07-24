using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Room.Events;

public class RoomCreatedEvent : DomainEventBase
{
    public RoomId RoomId { get; }
    public RoomNumber RoomNumber { get; }
    public Guid RoomTypeId { get; }

    public RoomCreatedEvent(RoomId roomId, RoomNumber roomNumber, Guid roomTypeId)
    {
        RoomId = roomId;
        RoomNumber = roomNumber;
        RoomTypeId = roomTypeId;
    }
}