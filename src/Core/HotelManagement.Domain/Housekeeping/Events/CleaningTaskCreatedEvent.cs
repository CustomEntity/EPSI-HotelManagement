using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Housekeeping.Events;

public class CleaningTaskCreatedEvent : DomainEventBase
{
    public CleaningTaskId CleaningTaskId { get; }
    public RoomId RoomId { get; }
    public CleaningPriority Priority { get; }
    public DateTime CreatedAt { get; }

    public CleaningTaskCreatedEvent(
        CleaningTaskId cleaningTaskId,
        RoomId roomId,
        CleaningPriority priority,
        DateTime createdAt)
    {
        CleaningTaskId = cleaningTaskId;
        RoomId = roomId;
        Priority = priority;
        CreatedAt = createdAt;
    }
}