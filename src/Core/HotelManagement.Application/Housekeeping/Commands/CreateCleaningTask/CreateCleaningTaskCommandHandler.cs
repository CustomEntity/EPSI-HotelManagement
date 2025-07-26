using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.Aggregates;
using HotelManagement.Domain.Housekeeping.Repositories;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Room;
using HotelManagement.Domain.Room.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Housekeeping.Commands.CreateCleaningTask;

public sealed class CreateCleaningTaskCommandHandler : IRequestHandler<CreateCleaningTaskCommand, Result<Guid>>
{
    private readonly ICleaningTaskRepository _cleaningTaskRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCleaningTaskCommandHandler> _logger;

    public CreateCleaningTaskCommandHandler(
        ICleaningTaskRepository cleaningTaskRepository,
        IRoomRepository roomRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateCleaningTaskCommandHandler> logger)
    {
        _cleaningTaskRepository = cleaningTaskRepository;
        _roomRepository = roomRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateCleaningTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating cleaning task for RoomId: {RoomId}, Priority: {Priority}",
                request.RoomId,
                request.Priority);

            var roomId = new RoomId(request.RoomId);
            var room = await _roomRepository.GetByIdAsync(roomId, cancellationToken);
            
            if (room == null)
            {
                _logger.LogWarning("Room not found with ID: {RoomId}", request.RoomId);
                return Result<Guid>.Failure("Room not found");
            }

            var hasPendingTask = await _cleaningTaskRepository.HasPendingTaskForRoomAsync(roomId);
            if (hasPendingTask)
            {
                _logger.LogWarning(
                    "Room {RoomId} already has a pending cleaning task",
                    request.RoomId);
                return Result<Guid>.Failure("Room already has a pending cleaning task");
            }

            var priorityResult = CreateCleaningPriority(request.Priority);
            if (priorityResult.IsFailure)
            {
                _logger.LogWarning(
                    "Invalid priority '{Priority}' provided for RoomId: {RoomId}",
                    request.Priority,
                    request.RoomId);
                return Result<Guid>.Failure(priorityResult.Error);
            }

            var cleaningTaskResult = CleaningTaskAggregate.Create(
                roomId,
                priorityResult.Value,
                request.ScheduledFor,
                request.Notes);

            if (cleaningTaskResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to create cleaning task for RoomId: {RoomId}, Error: {Error}",
                    request.RoomId,
                    cleaningTaskResult.Error);
                return Result<Guid>.Failure(cleaningTaskResult.Error);
            }

            var cleaningTask = cleaningTaskResult.Value;

            if (!string.IsNullOrWhiteSpace(request.AssignedTo))
            {
                var startResult = cleaningTask.Start(request.AssignedTo);
                if (startResult.IsFailure)
                {
                    _logger.LogWarning(
                        "Failed to assign cleaning task to '{AssignedTo}': {Error}",
                        request.AssignedTo,
                        startResult.Error);
                    return Result<Guid>.Failure(startResult.Error);
                }
            }

            await _cleaningTaskRepository.AddAsync(cleaningTask, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully created cleaning task {CleaningTaskId} for room {RoomId} with priority {Priority}",
                cleaningTask.Id.Value,
                request.RoomId,
                request.Priority);

            return Result<Guid>.Success(cleaningTask.Id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while creating cleaning task for RoomId: {RoomId}",
                request.RoomId);
            return Result<Guid>.Failure("An error occurred while creating the cleaning task");
        }
    }

    private static Result<CleaningPriority> CreateCleaningPriority(string priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
            return Result<CleaningPriority>.Failure("Priority cannot be null or empty");

        var cleaningPriority = CleaningPriority.FromString(priority);
        if (cleaningPriority == null)
            return Result<CleaningPriority>.Failure($"Invalid priority: {priority}. Valid values are: Low, Normal, High, Urgent");

        return Result<CleaningPriority>.Success(cleaningPriority);
    }
}