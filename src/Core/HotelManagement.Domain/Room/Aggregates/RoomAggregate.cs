using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room.Entities;
using HotelManagement.Domain.Room.Events;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;

namespace HotelManagement.Domain.Room.Aggregates;

public class Room : AggregateRoot<RoomId>
{
    public RoomNumber Number { get; private set; } = null!;
    public RoomType Type { get; private set; } = null!;
    public RoomStatus Status { get; private set; } = null!;
    public RoomCondition Condition { get; private set; } = null!;

    private Room()
    {
    }

    public Room(RoomNumber number, RoomType type, RoomCondition condition)
    {
        Id = RoomId.Create();
        Number = number;
        Type = type;
        Status = RoomStatus.Available;
        Condition = condition;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new RoomCreatedEvent(Id, Number, Type.Id));
    }

    public Result CheckIn()
    {
        if (Status != RoomStatus.Available)
            return Result.Failure($"Room is not available for check-in. Current status: {Status}");

        Status = RoomStatus.Occupied;
        ModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new RoomOccupiedEvent(Id, DateTime.UtcNow));
        return Result.Success();
    }

    public Result CheckOut()
    {
        if (Status != RoomStatus.Occupied)
            return Result.Failure($"Room is not occupied. Current status: {Status}");

        Status = RoomStatus.Cleaning;
        ModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new RoomVacatedEvent(Id, DateTime.UtcNow));
        return Result.Success();
    }

    public void MarkAsCleaned()
    {
        if (Status == RoomStatus.Cleaning)
        {
            Status = RoomStatus.Available;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    public void UpdateCondition(RoomCondition newCondition)
    {
        if (Condition != newCondition)
        {
            Condition = newCondition;
            ModifiedAt = DateTime.UtcNow;
        }
    }

    public void StartMaintenance()
    {
        Status = RoomStatus.Maintenance;
        ModifiedAt = DateTime.UtcNow;
    }

    public void EndMaintenance()
    {
        if (Status == RoomStatus.Maintenance)
        {
            Status = RoomStatus.Available;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}