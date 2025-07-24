using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Room.Events;

public class RoomVacatedEvent : DomainEventBase
{
    public RoomId RoomId { get; }
    public DateTime VacatedAt { get; }

    public RoomVacatedEvent(RoomId roomId, DateTime vacatedAt)
    {
        RoomId = roomId;
        VacatedAt = vacatedAt;
    }
}