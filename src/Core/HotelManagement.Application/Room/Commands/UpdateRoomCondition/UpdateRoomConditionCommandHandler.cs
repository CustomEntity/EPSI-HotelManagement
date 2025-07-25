using HotelManagement.Domain.Common;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;
using RoomAggregate = HotelManagement.Domain.Room.Aggregates.Room;

namespace HotelManagement.Application.Room.Commands.UpdateRoomCondition;

public sealed class UpdateRoomConditionCommandHandler : IRequestHandler<UpdateRoomConditionCommand, Result>
{
    private readonly IRoomRepository _roomRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoomConditionCommandHandler(
        IRoomRepository roomRepository,
        IUnitOfWork unitOfWork)
    {
        _roomRepository = roomRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateRoomConditionCommand request, CancellationToken cancellationToken)
    {
        var roomId = new RoomId(request.RoomId);

        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        if (room == null)
            return Result.Failure("Room not found");

        var newConditionResult = CreateRoomCondition(request.Condition);
        if (newConditionResult.IsFailure)
            return newConditionResult;

        var newCondition = newConditionResult.Value;

        if (room.Condition.Value == newCondition.Value)
            return Result.Failure("Room condition is already set to the specified value");

        var statusUpdateResult = HandleStatusChangeIfNeeded(room, newCondition);
        if (statusUpdateResult.IsFailure)
            return statusUpdateResult;

        room.UpdateCondition(newCondition);

        await _roomRepository.UpdateAsync(room, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Result<RoomCondition> CreateRoomCondition(string conditionValue)
    {
        var normalizedCondition = conditionValue?.Trim().ToLowerInvariant();
        
        return normalizedCondition switch
        {
            "new" => Result<RoomCondition>.Success(RoomCondition.New),
            "refurbished" => Result<RoomCondition>.Success(RoomCondition.Refurbished),
            "needsrefurbishment" or "needs refurbishment" => Result<RoomCondition>.Success(RoomCondition.NeedsRefurbishment),
            "good" => Result<RoomCondition>.Success(RoomCondition.Good),
            "damaged" => Result<RoomCondition>.Success(RoomCondition.Damaged),
            _ => Result<RoomCondition>.Failure($"Invalid room condition: {conditionValue}. Valid values are: New, Refurbished, NeedsRefurbishment, Good, Damaged")
        };
    }

    private static Result HandleStatusChangeIfNeeded(RoomAggregate room, RoomCondition newCondition)
    {
        if (newCondition == RoomCondition.Damaged && room.Status == RoomStatus.Available)
        {
            room.StartMaintenance();
            return Result.Success();
        }

        if (newCondition == RoomCondition.NeedsRefurbishment && room.Status == RoomStatus.Available)
        {
            room.StartMaintenance();
            return Result.Success();
        }

        if ((newCondition == RoomCondition.Good || newCondition == RoomCondition.New || newCondition == RoomCondition.Refurbished) 
            && room.Status == RoomStatus.Maintenance)
        {
            room.EndMaintenance();
            return Result.Success();
        }

        return Result.Success();
    }
}