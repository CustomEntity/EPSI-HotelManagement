using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room.ValueObjects;

namespace HotelManagement.Domain.Housekeeping.Events;

public class DamageReportedEvent : DomainEventBase
{
    public CleaningTaskId CleaningTaskId { get; }
    public RoomId RoomId { get; }
    public string DamageDescription { get; }
    public decimal? EstimatedRepairCost { get; }
    public DateTime ReportedAt { get; }

    public DamageReportedEvent(
        CleaningTaskId cleaningTaskId,
        RoomId roomId,
        string damageDescription,
        decimal? estimatedRepairCost,
        DateTime reportedAt)
    {
        CleaningTaskId = cleaningTaskId;
        RoomId = roomId;
        DamageDescription = damageDescription;
        EstimatedRepairCost = estimatedRepairCost;
        ReportedAt = reportedAt;
    }
}