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

        // Récupérer la chambre
        var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
        if (room == null)
            return Result.Failure("Room not found");

        // Valider et créer la nouvelle condition
        var newConditionResult = CreateRoomCondition(request.Condition);
        if (newConditionResult.IsFailure)
            return newConditionResult;

        var newCondition = newConditionResult.Value;

        // Vérifier si la condition a vraiment changé
        if (room.Condition.Value == newCondition.Value)
            return Result.Failure("Room condition is already set to the specified value");

        // Gérer les changements de statut si nécessaire
        var statusUpdateResult = HandleStatusChangeIfNeeded(room, newCondition);
        if (statusUpdateResult.IsFailure)
            return statusUpdateResult;

        // Mettre à jour la condition de la chambre
        room.UpdateCondition(newCondition);

        // Sauvegarder les changements
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
        // Si la chambre devient endommagée et qu'elle est disponible, la mettre en maintenance
        if (newCondition == RoomCondition.Damaged && room.Status == RoomStatus.Available)
        {
            room.StartMaintenance();
            return Result.Success();
        }

        // Si la chambre nécessite une rénovation et qu'elle est disponible, la mettre en maintenance
        if (newCondition == RoomCondition.NeedsRefurbishment && room.Status == RoomStatus.Available)
        {
            room.StartMaintenance();
            return Result.Success();
        }

        // Si la chambre est en maintenance et devient en bon état, terminer la maintenance
        if ((newCondition == RoomCondition.Good || newCondition == RoomCondition.New || newCondition == RoomCondition.Refurbished) 
            && room.Status == RoomStatus.Maintenance)
        {
            room.EndMaintenance();
            return Result.Success();
        }

        return Result.Success();
    }
}