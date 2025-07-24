using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Housekeeping.Events;

public class CleaningTaskStartedEvent : DomainEventBase
{
    public CleaningTaskId CleaningTaskId { get; }
    public RoomId RoomId { get; }
    public string AssignedTo { get; }
    public DateTime StartedAt { get; }

    public CleaningTaskStartedEvent(
        CleaningTaskId cleaningTaskId,
        RoomId roomId,
        string assignedTo,
        DateTime startedAt)
    {
        CleaningTaskId = cleaningTaskId;
        RoomId = roomId;
        AssignedTo = assignedTo;
        StartedAt = startedAt;
    }
}