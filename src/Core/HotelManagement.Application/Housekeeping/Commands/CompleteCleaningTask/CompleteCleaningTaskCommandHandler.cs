using HotelManagement.Domain.Common;
using HotelManagement.Domain.Housekeeping.Repositories;
using HotelManagement.Domain.Housekeeping.ValueObjects;
using HotelManagement.Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HotelManagement.Application.Housekeeping.Commands.CompleteCleaningTask;

public sealed class CompleteCleaningTaskCommandHandler : IRequestHandler<CompleteCleaningTaskCommand, Result>
{
    private readonly ICleaningTaskRepository _cleaningTaskRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompleteCleaningTaskCommandHandler> _logger;

    public CompleteCleaningTaskCommandHandler(
        ICleaningTaskRepository cleaningTaskRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompleteCleaningTaskCommandHandler> logger)
    {
        _cleaningTaskRepository = cleaningTaskRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CompleteCleaningTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Completing cleaning task with ID: {CleaningTaskId}",
                request.CleaningTaskId);

            var cleaningTaskId = CleaningTaskId.Create(request.CleaningTaskId);
            var cleaningTask = await _cleaningTaskRepository.GetByIdAsync(cleaningTaskId, cancellationToken);

            if (cleaningTask == null)
            {
                _logger.LogWarning(
                    "Cleaning task not found with ID: {CleaningTaskId}",
                    request.CleaningTaskId);
                return Result.Failure("Cleaning task not found");
            }

            if (cleaningTask.Status != CleaningStatus.InProgress)
            {
                _logger.LogWarning(
                    "Cannot complete cleaning task {CleaningTaskId} with status {Status}",
                    request.CleaningTaskId,
                    cleaningTask.Status.Value);
                return Result.Failure($"Cannot complete cleaning task with status {cleaningTask.Status.Value}");
            }

            var completeResult = cleaningTask.Complete(request.CompletionNotes);
            if (completeResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to complete cleaning task {CleaningTaskId}: {Error}",
                    request.CleaningTaskId,
                    completeResult.Error);
                return Result.Failure(completeResult.Error);
            }

            await _cleaningTaskRepository.UpdateAsync(cleaningTask, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully completed cleaning task {CleaningTaskId} for room {RoomId} by {AssignedTo}",
                cleaningTask.Id.Value,
                cleaningTask.RoomId.Value,
                cleaningTask.AssignedTo ?? "Unknown");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error occurred while completing cleaning task {CleaningTaskId}",
                request.CleaningTaskId);
            return Result.Failure("An error occurred while completing the cleaning task");
        }
    }
}