using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Housekeeping.Events;

public class CleaningTaskCancelledEvent : DomainEventBase
{
    public CleaningTaskId CleaningTaskId { get; }
    public RoomId RoomId { get; }
    public string Reason { get; }
    public DateTime CancelledAt { get; }

    public CleaningTaskCancelledEvent(
        CleaningTaskId cleaningTaskId,
        RoomId roomId,
        string reason,
        DateTime cancelledAt)
    {
        CleaningTaskId = cleaningTaskId;
        RoomId = roomId;
        Reason = reason;
        CancelledAt = cancelledAt;
    }
}