using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Room.Events;

public class RoomOccupiedEvent : DomainEventBase
{
    public RoomId RoomId { get; }
    public DateTime OccupiedAt { get; }

    public RoomOccupiedEvent(RoomId roomId, DateTime occupiedAt)
    {
        RoomId = roomId;
        OccupiedAt = occupiedAt;
    }
}