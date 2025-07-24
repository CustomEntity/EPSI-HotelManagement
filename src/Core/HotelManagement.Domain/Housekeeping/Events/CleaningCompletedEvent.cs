using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Housekeeping.Events;

public class CleaningCompletedEvent : DomainEventBase
{
    public CleaningTaskId CleaningTaskId { get; }
    public RoomId RoomId { get; }
    public DateTime CompletedAt { get; }
    public string CompletedBy { get; }

    public CleaningCompletedEvent(
        CleaningTaskId cleaningTaskId,
        RoomId roomId,
        DateTime completedAt,
        string completedBy)
    {
        CleaningTaskId = cleaningTaskId;
        RoomId = roomId;
        CompletedAt = completedAt;
        CompletedBy = completedBy;
    }
}